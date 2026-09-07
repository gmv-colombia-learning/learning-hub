using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text.Json;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Infraestructure.data;
using VirtualBuddy.Infraestructure.Util;

namespace VirtualBuddy.Infraestructure.Services
{
    public sealed class SqlServerKnowledgeBaseService : IKnowledgeBaseService
    {
        private readonly BuddyDBContext _context;
        private readonly int _embeddingDimension;

        public SqlServerKnowledgeBaseService(
            BuddyDBContext context,
            IOptions<EmbeddingSettings> settings)
        {
            _context = context;
            _embeddingDimension = settings.Value.EmbeddingDimension;
        }

        public async Task AddChunksAsync(
            Guid projectId,
            Guid? documentId,
            IEnumerable<(string Content, float[] Embedding, string? Metadata)> chunks)
        {
            var chunkList = chunks.ToList();
            foreach (var chunk in chunkList)
                EmbeddingValidator.Validate(chunk.Embedding, _embeddingDimension);

            if (chunkList.Count == 0)
                return;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var sql = $"""
                INSERT INTO [KnowledgeChunks] ([Id], [ProjectId], [DocumentId], [Content], [Embedding], [Metadata])
                VALUES (@id, @projectId, @documentId, @content, CAST(@embedding AS VECTOR({_embeddingDimension})), @metadata)
                """;

            foreach (var chunk in chunkList)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    sql,
                    new SqlParameter("@id", SqlDbType.UniqueIdentifier) { Value = Guid.NewGuid() },
                    new SqlParameter("@projectId", SqlDbType.UniqueIdentifier) { Value = projectId },
                    new SqlParameter("@documentId", SqlDbType.UniqueIdentifier) { Value = (object?)documentId ?? DBNull.Value },
                    new SqlParameter("@content", SqlDbType.NVarChar, -1) { Value = chunk.Content },
                    new SqlParameter("@embedding", SqlDbType.NVarChar, -1) { Value = JsonSerializer.Serialize(chunk.Embedding) },
                    new SqlParameter("@metadata", SqlDbType.NVarChar, -1) { Value = (object?)chunk.Metadata ?? DBNull.Value });
            }

            await transaction.CommitAsync();
        }

        public async Task<IEnumerable<string>> SearchRelevantChunksAsync(
            Guid projectId,
            float[] queryEmbedding,
            int limit = 5)
        {
            EmbeddingValidator.Validate(queryEmbedding, _embeddingDimension);
            if (limit <= 0)
                return [];

            var results = new List<string>();
            await using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = $"""
                SELECT TOP (@limit) [Content]
                FROM [KnowledgeChunks]
                WHERE [ProjectId] = @projectId
                ORDER BY VECTOR_DISTANCE('cosine', [Embedding], CAST(@queryEmbedding AS VECTOR({_embeddingDimension}))) ASC
                """;
            command.Parameters.Add(new SqlParameter("@projectId", SqlDbType.UniqueIdentifier) { Value = projectId });
            command.Parameters.Add(new SqlParameter("@queryEmbedding", SqlDbType.NVarChar, -1)
            {
                Value = JsonSerializer.Serialize(queryEmbedding)
            });
            command.Parameters.Add(new SqlParameter("@limit", SqlDbType.Int) { Value = limit });

            var shouldCloseConnection = command.Connection!.State != ConnectionState.Open;
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

            return results;
        }

        public Task DeleteChunksByDocumentIdAsync(Guid documentId)
        {
            const string sql = "DELETE FROM [KnowledgeChunks] WHERE [DocumentId] = @documentId";
            return _context.Database.ExecuteSqlRawAsync(
                sql,
                new SqlParameter("@documentId", SqlDbType.UniqueIdentifier) { Value = documentId });
        }
    }
}
