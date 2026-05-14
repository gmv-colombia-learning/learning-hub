using VirtualBuddy.Application.AI.UseCases;
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
        private readonly IndexDocument _indexDocument;

        public UploadDocument(
            IRepository repository,
            IFileStorageService fileStorageService,
            IndexDocument indexDocument)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
            _indexDocument = indexDocument;
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

            // Create a copy of the stream for indexing because some storage services might close it
            using var indexStream = new MemoryStream();
            await fileStream.CopyToAsync(indexStream);
            fileStream.Position = 0;
            indexStream.Position = 0;

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

            // 5. Trigger AI Indexation for supported document types
            bool isSupportedByAI = contentType.Contains("pdf") || 
                                   contentType.Contains("word") || 
                                   contentType.Contains("excel") || 
                                   contentType.Contains("officedocument") || 
                                   contentType.Contains("text/plain");

            if (document.Type == DocumentType.File && isSupportedByAI)
            {
                try
                {
                    await _indexDocument.ExecuteAsync(projectId, document.Id, indexStream, fileName);
                }
                catch (Exception)
                {
                    // For now, we don't want to fail the upload if indexing fails
                    // In a production app, we might want to log this or use a background job
                }
            }

            // 6. Mapear a DTO
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
