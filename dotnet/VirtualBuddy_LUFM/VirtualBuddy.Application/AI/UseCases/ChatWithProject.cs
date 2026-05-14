using System.Text;
using VirtualBuddy.Application.Common.Interfaces;

namespace VirtualBuddy.Application.AI.UseCases
{
    public class ChatWithProject
    {
        private readonly IAIService _aiService;
        private readonly IKnowledgeBaseService _knowledgeBaseService;

        public ChatWithProject(
            IAIService aiService,
            IKnowledgeBaseService knowledgeBaseService)
        {
            _aiService = aiService;
            _knowledgeBaseService = knowledgeBaseService;
        }

        public async Task<string> ExecuteAsync(Guid projectId, string userQuestion)
        {
            // Generar embedding de la pregunta
            var queryEmbedding = await _aiService.GetEmbeddingAsync(userQuestion);

            var relevantChunks = await _knowledgeBaseService.SearchRelevantChunksAsync(projectId, queryEmbedding);

            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("Eres VirtualBuddy, un asistente experto en mentoría de proyectos de desarrollo de software.");
            contextBuilder.AppendLine("Usa la siguiente información del proyecto para responder la pregunta del usuario.");
            contextBuilder.AppendLine("Si la información no está en el contexto, di que no lo sabes, no inventes información.");
            contextBuilder.AppendLine("\n--- CONTEXTO DEL PROYECTO ---");

            foreach (var chunk in relevantChunks)
            {
                contextBuilder.AppendLine(chunk);
            }
            contextBuilder.AppendLine("--- FIN DEL CONTEXTO ---\n");

            //Obtener respuesta del LLM
            return await _aiService.GetChatResponseAsync(contextBuilder.ToString(), userQuestion);
        }
    }
}
