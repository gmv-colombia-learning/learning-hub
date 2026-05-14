namespace VirtualBuddy.Application.Common.Interfaces
{
    public interface IKnowledgeBaseService
    {
        Task AddChunksAsync(Guid projectId, Guid? documentId, IEnumerable<(string Content, float[] Embedding, string? Metadata)> chunks);
        Task<IEnumerable<string>> SearchRelevantChunksAsync(Guid projectId, float[] queryEmbedding, int limit = 5);
        Task DeleteChunksByDocumentIdAsync(Guid documentId);
    }
}
