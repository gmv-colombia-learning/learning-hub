namespace VirtualBuddy.Application.Common.Interfaces
{
    public interface IProjectImageStorageService
    {
        Task<string> UploadAsync(Guid projectId, Stream imageStream, string contentType);
        Task DeleteAsync(Guid projectId, string url);
        bool IsManagedUrl(Guid projectId, string url);
    }
}
