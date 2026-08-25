using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text.Json;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Infraestructure.data;

namespace VirtualBuddy.Infraestructure.Services
{
    public class PostgresKnowledgeBaseService : IKnowledgeBaseService
    {
        private readonly BuddyDBContext _context;

        public PostgresKnowledgeBaseService(BuddyDBContext context)
        {
            _context = context;
        }

        public async Task AddChunksAsync(Guid projectId, Guid? documentId, IEnumerable<(string Content, float[] Embedding, string? Metadata)> chunks)
        {
            foreach (var chunk in chunks)
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
        }

        public async Task<IEnumerable<string>> SearchRelevantChunksAsync(Guid projectId, float[] queryEmbedding, int limit = 5)
        {
            // Since pgvector is not available, we use a manual cosine similarity calculation in SQL
            // Cosine Similarity = (A . B) / (||A|| * ||B||)
            // For normalized embeddings, it's just the dot product (A . B)
            
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
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = sql;
                command.Parameters.Add(new NpgsqlParameter("@projectId", projectId));
                command.Parameters.Add(new NpgsqlParameter("@queryEmbedding", queryEmbedding));
                command.Parameters.Add(new NpgsqlParameter("@limit", limit));

                if (command.Connection != null && command.Connection.State != System.Data.ConnectionState.Open)
                    await command.Connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        results.Add(reader.GetString(0));
                    }
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
