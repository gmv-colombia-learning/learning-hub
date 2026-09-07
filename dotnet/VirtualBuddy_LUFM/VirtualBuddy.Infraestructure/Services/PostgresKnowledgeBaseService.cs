using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.Extensions.Options;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Infraestructure.data;
using VirtualBuddy.Infraestructure.Util;

namespace VirtualBuddy.Infraestructure.Services
{
    public class PostgresKnowledgeBaseService : IKnowledgeBaseService
    {
        private readonly BuddyDBContext _context;
        private readonly int _embeddingDimension;

        public PostgresKnowledgeBaseService(
            BuddyDBContext context,
            IOptions<EmbeddingSettings> settings)
        {
            _context = context;
            _embeddingDimension = settings.Value.EmbeddingDimension;
        }

        public async Task AddChunksAsync(Guid projectId, Guid? documentId, IEnumerable<(string Content, float[] Embedding, string? Metadata)> chunks)
        {
            var chunkList = chunks.ToList();
            foreach (var chunk in chunkList)
                EmbeddingValidator.Validate(chunk.Embedding, _embeddingDimension);

            if (chunkList.Count == 0)
                return;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            foreach (var chunk in chunkList)
            {
                var sql = @"
                    INSERT INTO ""KnowledgeChunks"" (""Id"", ""ProjectId"", ""DocumentId"", ""Content"", ""Embedding"", ""Metadata"")
                    VALUES (@id, @projectId, @documentId, @content, @embedding, @metadata::jsonb)";

                await _context.Database.ExecuteSqlRawAsync(sql,
                    new NpgsqlParameter("@id", Guid.NewGuid()),
                    new NpgsqlParameter("@projectId", projectId),
                    new NpgsqlParameter("@documentId", (object?)documentId ?? DBNull.Value),
                    new NpgsqlParameter("@content", chunk.Content),
                    new NpgsqlParameter("@embedding", chunk.Embedding),
                    new NpgsqlParameter("@metadata", chunk.Metadata ?? "{}")
                );
            }

            await transaction.CommitAsync();
        }

        public async Task<IEnumerable<string>> SearchRelevantChunksAsync(Guid projectId, float[] queryEmbedding, int limit = 5)
        {
            EmbeddingValidator.Validate(queryEmbedding, _embeddingDimension);
            if (limit <= 0)
                return [];
            
            var sql = @"
                SELECT ""Content""
                FROM (
                    SELECT ""Content"", 
                           (
                             SELECT SUM(a * b)
                             FROM UNNEST(""Embedding"", @queryEmbedding) AS x(a, b)
                           ) as similarity
                    FROM ""KnowledgeChunks""
                    WHERE ""ProjectId"" = @projectId
                ) AS sub
                ORDER BY similarity DESC
                LIMIT @limit";

            var results = new List<string>();
            await using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.Add(new NpgsqlParameter("@projectId", projectId));
                command.Parameters.Add(new NpgsqlParameter("@queryEmbedding", queryEmbedding));
                command.Parameters.Add(new NpgsqlParameter("@limit", limit));

                var shouldCloseConnection = command.Connection!.State != System.Data.ConnectionState.Open;
                if (shouldCloseConnection)
                    await command.Connection.OpenAsync();

                try
                {
                    await using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                        results.Add(reader.GetString(0));
                }
                finally
                {
                    if (shouldCloseConnection)
                        await command.Connection.CloseAsync();
                }
            }

            return results;
        }

        public async Task DeleteChunksByDocumentIdAsync(Guid documentId)
        {
            var sql = @"DELETE FROM ""KnowledgeChunks"" WHERE ""DocumentId"" = @documentId";
            await _context.Database.ExecuteSqlRawAsync(sql, new NpgsqlParameter("@documentId", documentId));
        }
    }
}
