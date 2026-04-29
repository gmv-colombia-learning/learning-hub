using VirtualBuddy.Domain.Common.Enums;

namespace VirtualBuddy.Domain.Document
{
    public class Document
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Size { get; set; }
        public DateTime UploadedAt { get; set; }
        public DocumentType Type { get; set; }
        public string? Location { get; set; }
        public string? URLLocation { get; set; }
        public string? ExtractedAI { get; set; }
    }
}
