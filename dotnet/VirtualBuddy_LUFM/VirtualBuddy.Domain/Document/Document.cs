using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Enums;

namespace VirtualBuddy.Domain.Document
{
    public class Document : Entity
    {
        public string Name { get; private set; }
        public string? Description { get; private set; }
        public string Size { get; private set; }
        public DateTime UploadedAt { get; private set; }
        public DocumentType Type { get; private set; }
        public Guid ProjectId { get; private set; }
        public string StoragePath { get; private set; }
        public string PublicUrl { get; private set; }
        public string ContentType { get; private set; }
        public string? ExtractedAI { get; private set; }

        // Constructor para EF Core
        private Document() { }

        private Document(
            string name, 
            string size, 
            DocumentType type, 
            Guid projectId, 
            string storagePath, 
            string publicUrl, 
            string contentType,
            string? description = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            Size = size;
            Type = type;
            ProjectId = projectId;
            StoragePath = storagePath;
            PublicUrl = publicUrl;
            ContentType = contentType;
            Description = description;
            UploadedAt = DateTime.UtcNow;
        }

        public static Document Create(
            string name, 
            string size, 
            DocumentType type, 
            Guid projectId, 
            string storagePath, 
            string publicUrl, 
            string contentType,
            string? description = null)
        {
            // Aquí se podrían añadir validaciones de dominio si fueran necesarias
            return new Document(name, size, type, projectId, storagePath, publicUrl, contentType, description);
        }

        public void SetExtractedAI(string extractedText)
        {
            ExtractedAI = extractedText;
        }

        public void UpdateDescription(string description)
        {
            Description = description;
        }
    }
}
