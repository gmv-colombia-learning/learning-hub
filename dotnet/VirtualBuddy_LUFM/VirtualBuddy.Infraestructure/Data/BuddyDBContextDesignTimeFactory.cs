using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace VirtualBuddy.Infraestructure.data
{
    public sealed class BuddyDBContextDesignTimeFactory : IDesignTimeDbContextFactory<BuddyDBContext>
    {
        public BuddyDBContext CreateDbContext(string[] args)
        {
            var provider = GetArgument(args, "--provider")
                ?? throw new InvalidOperationException(
                    "El argumento '--provider PostgreSql|SqlServer' es obligatorio para Entity Framework.");
            var environmentName = provider switch
            {
                "PostgreSql" => "Local",
                "SqlServer" => "Development",
                _ => throw new InvalidOperationException(
                    $"El proveedor '{provider}' no esta soportado. Use 'PostgreSql' o 'SqlServer'.")
            };

            var apiDirectory = ResolveApiDirectory();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: false)
                .AddEnvironmentVariables()
                .Build();
            var connectionString = configuration.GetConnectionString(environmentName)
                ?? throw new InvalidOperationException(
                    $"La configuracion 'ConnectionStrings:{environmentName}' es obligatoria.");

            var options = new DbContextOptionsBuilder<BuddyDBContext>();
            if (provider == "PostgreSql")
            {
                options.UseNpgsql(
                    connectionString,
                    database => database.MigrationsAssembly("VirtualBuddy.Infraestructure"));
            }
            else
            {
                options.UseSqlServer(
                    connectionString,
                    database => database
                        .MigrationsAssembly("VirtualBuddy.Migrations.SqlServer")
                        .EnableRetryOnFailure());
            }

            return new BuddyDBContext(options.Options);
        }

        private static string? GetArgument(string[] args, string name)
        {
            var index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }

        private static string ResolveApiDirectory()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var nestedApiDirectory = Path.Combine(currentDirectory, "VirtualBuddy.Api");
            if (Directory.Exists(nestedApiDirectory))
                return nestedApiDirectory;

            var siblingApiDirectory = Path.GetFullPath(Path.Combine(currentDirectory, "..", "VirtualBuddy.Api"));
            if (Directory.Exists(siblingApiDirectory))
                return siblingApiDirectory;

            throw new InvalidOperationException("No se encontro el directorio VirtualBuddy.Api.");
        }
    }
}
