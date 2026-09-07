using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using VirtualBuddy.Infraestructure.Util;

namespace VirtualBuddy.Infraestructure.Data
{
    public static class DatabaseStartup
    {
        public static async Task ValidateAndSeedAsync(
            data.BuddyDBContext context,
            Microsoft.AspNetCore.Identity.UserManager<Identity.ApplicationUser> userManager,
            IOptions<EmbeddingSettings> embeddingSettings,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await context.Database.CanConnectAsync(cancellationToken))
                    throw new InvalidOperationException(
                        "No fue posible conectar con la base de datos. Verifique credenciales y reglas de red.");
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch
            {
                throw new InvalidOperationException(
                    "No fue posible conectar con la base de datos. Verifique credenciales y reglas de red.");
            }

            IEnumerable<string> pendingMigrations;
            try
            {
                pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            }
            catch
            {
                throw new InvalidOperationException(
                    "No fue posible validar el historial de migraciones de la base de datos.");
            }
            if (pendingMigrations.Any())
                throw new InvalidOperationException(
                    "La base de datos tiene migraciones pendientes. Aplique las migraciones del proveedor antes de iniciar la API.");

            if (context.Database.IsSqlServer())
                await ValidateSqlServerVectorSchemaAsync(
                    context,
                    embeddingSettings.Value.EmbeddingDimension,
                    cancellationToken);

            try
            {
                await DbInitializer.SeedAsync(context, userManager);
            }
            catch
            {
                throw new InvalidOperationException(
                    "No fue posible ejecutar el seed de la base de datos.");
            }
        }

        private static async Task ValidateSqlServerVectorSchemaAsync(
            data.BuddyDBContext context,
            int expectedDimension,
            CancellationToken cancellationToken)
        {
            try
            {
                await using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = """
                    SELECT c.vector_dimensions
                    FROM sys.columns AS c
                    INNER JOIN sys.tables AS t ON t.object_id = c.object_id
                    INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
                    WHERE s.name = 'dbo' AND t.name = 'KnowledgeChunks' AND c.name = 'Embedding'
                    """;

                if (command.Connection!.State != ConnectionState.Open)
                    await command.Connection.OpenAsync(cancellationToken);

                var dimensionValue = await command.ExecuteScalarAsync(cancellationToken);
                if (dimensionValue is null or DBNull)
                    throw new InvalidOperationException();

                var actualDimension = Convert.ToInt32(dimensionValue);
                if (actualDimension != expectedDimension)
                    throw new InvalidOperationException(
                        $"La dimension vectorial configurada ({expectedDimension}) no coincide con el esquema Azure SQL ({actualDimension}).");

                command.CommandText = $"""
                    SELECT VECTOR_DISTANCE(
                        'cosine',
                        CAST(@leftVector AS VECTOR({expectedDimension})),
                        CAST(@rightVector AS VECTOR({expectedDimension})))
                    """;
                command.Parameters.Add(new SqlParameter("@leftVector", SqlDbType.NVarChar, -1)
                {
                    Value = CreateUnitVectorJson(expectedDimension)
                });
                command.Parameters.Add(new SqlParameter("@rightVector", SqlDbType.NVarChar, -1)
                {
                    Value = CreateUnitVectorJson(expectedDimension)
                });
                _ = await command.ExecuteScalarAsync(cancellationToken);
            }
            catch (InvalidOperationException exception) when (exception.Message.StartsWith("La dimension vectorial"))
            {
                throw;
            }
            catch
            {
                throw new InvalidOperationException(
                    "El esquema Azure SQL no contiene una columna KnowledgeChunks.Embedding vectorial compatible.");
            }
        }

        private static string CreateUnitVectorJson(int dimension)
        {
            var vector = new float[dimension];
            vector[0] = 1;
            return System.Text.Json.JsonSerializer.Serialize(vector);
        }
    }
}
