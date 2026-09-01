using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Application.Project
{
    public class ProjectImageService
    {
        public const long MaxFileSize = 5 * 1024 * 1024;

        private readonly IProjectImageStorageService _storage;

        public ProjectImageService(IProjectImageStorageService storage)
        {
            _storage = storage;
        }

        public async Task<string> UploadAsync(
            Guid projectId,
            Stream imageStream,
            string fileName,
            string contentType,
            long fileSize)
        {
            if (fileSize <= 0)
                throw new ValidationException("Project image cannot be empty.");
            if (fileSize > MaxFileSize)
                throw new ValidationException("Project image cannot exceed 5 MB.");
            var effectiveContentType = ResolveContentType(fileName, contentType);

            using var content = new MemoryStream();
            await imageStream.CopyToAsync(content);
            if (content.Length != fileSize || !HasValidSignature(content.GetBuffer(), effectiveContentType))
                throw new ValidationException("Project image content does not match its declared format.");

            content.Position = 0;
            try
            {
                return await _storage.UploadAsync(projectId, content, effectiveContentType);
            }
            catch (Exception exception) when (exception is not TemporaryServiceUnavailableException)
            {
                throw new TemporaryServiceUnavailableException("Project image storage is temporarily unavailable.");
            }
        }

        public bool IsManagedUrl(Guid projectId, string? url) =>
            url != null && _storage.IsManagedUrl(projectId, url);

        public async Task DeleteIfManagedAsync(Guid projectId, string? url)
        {
            if (!IsManagedUrl(projectId, url))
                return;

            try
            {
                await _storage.DeleteAsync(projectId, url!);
            }
            catch (Exception exception) when (exception is not TemporaryServiceUnavailableException)
            {
                throw new TemporaryServiceUnavailableException("Project image storage is temporarily unavailable.");
            }
        }

        private static string ResolveContentType(string fileName, string declaredContentType)
        {
            var contentTypeFromExtension = Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => null
            };

            if (contentTypeFromExtension == null ||
                (!string.Equals(declaredContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase) &&
                 !string.Equals(declaredContentType, contentTypeFromExtension, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ValidationException("Project image must be a JPEG, PNG, or WebP file.");
            }

            return contentTypeFromExtension;
        }

        private static bool HasValidSignature(byte[] content, string contentType)
        {
            return contentType switch
            {
                "image/jpeg" => content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF,
                "image/png" => content.Length >= 8 && content.AsSpan(0, 8).SequenceEqual(
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
                "image/webp" => content.Length >= 12 &&
                    content.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                    content.AsSpan(8, 4).SequenceEqual("WEBP"u8),
                _ => false
            };
        }
    }
}
