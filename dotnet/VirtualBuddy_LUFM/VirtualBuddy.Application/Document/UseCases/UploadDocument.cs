using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UploadDocument> _logger;

        public UploadDocument(
            IRepository repository,
            IFileStorageService fileStorageService,
            IndexDocument indexDocument,
            ILogger<UploadDocument> logger)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
            _indexDocument = indexDocument;
            _logger = logger;
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

            var effectiveContentType = ResolveContentType(fileName, contentType);

            // Create a copy of the stream for indexing because some storage services might close it
            using var indexStream = new MemoryStream();
            await fileStream.CopyToAsync(indexStream);
            fileStream.Position = 0;
            indexStream.Position = 0;

            // 2. Subir archivo al storage
            var storagePath = await _fileStorageService.UploadFileAsync(fileName, fileStream, effectiveContentType);
            var publicUrl = await _fileStorageService.GetSignedUrlAsync(storagePath);

            // 3. Crear entidad de dominio
            var document = VirtualBuddy.Domain.Document.Document.Create(
                name: fileName,
                size: FormatFileSize(fileSize),
                type: MapContentTypeToDocumentType(effectiveContentType),
                projectId: projectId,
                storagePath: storagePath,
                publicUrl: publicUrl,
                contentType: effectiveContentType,
                description: description
            );

            // 4. Persistir en DB
            await _repository.AddAsync(document);
            await _repository.SaveChangesAsync();

            // 5. Trigger AI Indexation for supported document types
            bool isSupportedByAI = IsSupportedByAI(effectiveContentType);

            if (document.Type == DocumentType.File && isSupportedByAI)
            {
                try
                {
                    await _indexDocument.ExecuteAsync(projectId, document.Id, indexStream, fileName);
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        "No se pudo indexar el documento {DocumentId}. Tipo: {ExceptionType}",
                        document.Id,
                        exception.GetType().Name);
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

        private static string ResolveContentType(string fileName, string declaredContentType)
        {
            if (!string.Equals(
                    declaredContentType,
                    "application/octet-stream",
                    StringComparison.OrdinalIgnoreCase))
            {
                return declaredContentType;
            }

            return Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".xls" => "application/vnd.ms-excel",
                ".txt" => "text/plain",
                _ => declaredContentType
            };
        }

        private static bool IsSupportedByAI(string contentType)
        {
            return contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase) ||
                   contentType.Contains("word", StringComparison.OrdinalIgnoreCase) ||
                   contentType.Contains("excel", StringComparison.OrdinalIgnoreCase) ||
                   contentType.Contains("officedocument", StringComparison.OrdinalIgnoreCase) ||
                   contentType.Contains("text/plain", StringComparison.OrdinalIgnoreCase);
        }

        private static DocumentType MapContentTypeToDocumentType(string contentType)
        {
            if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) return DocumentType.Image;
            if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)) return DocumentType.Video;
            if (IsSupportedByAI(contentType)) return DocumentType.File;
            return DocumentType.Unknown;
        }
    }
}
