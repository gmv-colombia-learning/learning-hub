namespace VirtualBuddy.Application.DTOs.Response
{
    public class DocumentResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Size { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string Type { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public string PublicUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
    }
}
