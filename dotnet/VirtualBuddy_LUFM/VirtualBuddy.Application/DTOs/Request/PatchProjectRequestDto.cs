using VirtualBuddy.Domain.Common.Enums;

namespace VirtualBuddy.Application.DTOs.Request
{
    public class PatchProjectRequestDto
    {
        public string? Name { get; set; }
        public string? Acronym { get; set; }
        public string? Description { get; set; }
        public DateTime? DevelopmentTime { get; set; }
        public ProjectStatus? Status { get; set; }
        public string? UrlImage { get; set; }
    }
}
