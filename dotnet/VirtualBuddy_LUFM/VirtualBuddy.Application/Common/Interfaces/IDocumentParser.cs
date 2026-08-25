namespace VirtualBuddy.Application.Common.Interfaces
{
    public interface IDocumentParser
    {
        Task<string> ExtractTextAsync(Stream fileStream, string fileName);
    }
}
