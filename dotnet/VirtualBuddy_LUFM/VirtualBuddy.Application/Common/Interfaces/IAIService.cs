namespace VirtualBuddy.Application.Common.Interfaces
{
    public interface IAIService
    {
        Task<string> GetChatResponseAsync(string systemPrompt, string userPrompt);
        Task<float[]> GetEmbeddingAsync(string text);
    }
}
