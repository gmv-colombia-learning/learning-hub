using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Enums;

namespace VirtualBuddy.Application.Document.UseCases
{
    public class UploadDocument
    {
        private readonly IRepository _repository;
        private readonly IFileStorageService _fileStorageService;

        public UploadDocument(
            IRepository repository,
            IFileStorageService fileStorageService)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
        }

        public async Task<DocumentResponseDto> ExecuteAsync(
            Guid projectId,
            Stream fileStream,
            string fileName,
            string contentType,
            long fileSize,
            string? description = null)
        {
            // 1. Validar existencia del proyecto
            var project = await _repository.GetByIdAsync<Domain.Project.Project>(projectId);
            if (project == null)
                throw new KeyNotFoundException($"Project with ID {projectId} not found.");

            // 2. Subir archivo al storage
            var storagePath = await _fileStorageService.UploadFileAsync(fileName, fileStream, contentType);
            var publicUrl = await _fileStorageService.GetSignedUrlAsync(storagePath);

            // 3. Crear entidad de dominio
            var document = VirtualBuddy.Domain.Document.Document.Create(
                name: fileName,
                size: FormatFileSize(fileSize),
                type: MapContentTypeToDocumentType(contentType),
                projectId: projectId,
                storagePath: storagePath,
                publicUrl: publicUrl,
                contentType: contentType,
                description: description
            );

            // 4. Persistir en DB
            await _repository.AddAsync(document);
            await _repository.SaveChangesAsync();

            // 5. Mapear a DTO
            return new DocumentResponseDto
            {
                Id = document.Id,
                Name = document.Name,
                Description = document.Description,
                Size = document.Size,
                UploadedAt = document.UploadedAt,
                Type = document.Type.ToString(),
                ProjectId = document.ProjectId,
                PublicUrl = document.PublicUrl,
                ContentType = document.ContentType
            };
        }

        private string FormatFileSize(long bytes)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB" };
            if (bytes == 0) return "0" + suf[0];
            long place = Convert.ToInt64(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return num.ToString() + suf[place];
        }

        private DocumentType MapContentTypeToDocumentType(string contentType)
        {
            if (contentType.StartsWith("image/")) return DocumentType.Image;
            if (contentType.StartsWith("video/")) return DocumentType.Video;
            if (contentType.Contains("pdf") || contentType.Contains("word") || contentType.Contains("text")) return DocumentType.File;
            return DocumentType.Unknown;
        }
    }
}
