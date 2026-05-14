using VirtualBuddy.Application.Common.Interfaces;

namespace VirtualBuddy.Application.AI.UseCases
{
    public class IndexDocument
    {
        private readonly IDocumentParser _documentParser;
        private readonly IAIService _aiService;
        private readonly IKnowledgeBaseService _knowledgeBaseService;

        public IndexDocument(
            IDocumentParser documentParser,
            IAIService aiService,
            IKnowledgeBaseService knowledgeBaseService)
        {
            _documentParser = documentParser;
            _aiService = aiService;
            _knowledgeBaseService = knowledgeBaseService;
        }

        public async Task ExecuteAsync(Guid projectId, Guid documentId, Stream fileStream, string fileName)
        {
            // 1. Extraer texto
            var text = await _documentParser.ExtractTextAsync(fileStream, fileName);
            if (string.IsNullOrWhiteSpace(text)) return;

            // 2. Chunking (Simple implementation, can be improved)
            var chunks = SplitIntoChunks(text, 1000);

            var chunkData = new List<(string Content, float[] Embedding, string? Metadata)>();

            foreach (var chunk in chunks)
            {
                // 3. Generar embedding para cada chunk
                var embedding = await _aiService.GetEmbeddingAsync(chunk);
                chunkData.Add((chunk, embedding, $@"{{""source"": ""{fileName}""}}"));
            }

            // 4. Guardar en Vector Store (Postgres)
            await _knowledgeBaseService.AddChunksAsync(projectId, documentId, chunkData);
        }

        private IEnumerable<string> SplitIntoChunks(string text, int chunkSize)
        {
            // Simple overlap chunking
            int overlap = 100;
            for (int i = 0; i < text.Length; i += (chunkSize - overlap))
            {
                yield return text.Substring(i, Math.Min(chunkSize, text.Length - i));
                if (i + chunkSize >= text.Length) break;
            }
        }
    }
}
