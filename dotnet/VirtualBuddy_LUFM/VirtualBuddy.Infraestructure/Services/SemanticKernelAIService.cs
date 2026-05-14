using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using VirtualBuddy.Application.Common.Interfaces;

namespace VirtualBuddy.Infraestructure.Services
{
    public class SemanticKernelAIService : IAIService
    {
        private readonly Kernel _kernel;

        public SemanticKernelAIService(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<string> GetChatResponseAsync(string systemPrompt, string userPrompt)
        {
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();

            var history = new ChatHistory(systemPrompt);
            history.AddUserMessage(userPrompt);

            using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));

            var result = await chatService.GetChatMessageContentAsync(history, cancellationToken: cts.Token);

            return result.ToString();
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var embeddingService = _kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var result = await embeddingService.GenerateAsync(new List<string> { text });
            return result[0].Vector.ToArray();
        }
    }
}
