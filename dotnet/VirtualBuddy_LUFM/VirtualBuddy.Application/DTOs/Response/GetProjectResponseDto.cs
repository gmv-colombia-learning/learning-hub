using VirtualBuddy.Domain.Common.Enums;
using VirtualBuddy.Application.DTOs.Shared;

namespace VirtualBuddy.Application.DTOs.Response
{
    public class GetProjectResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Acronym { get; set; }
        public string Description { get; set; }
        public DateTime DevelopmentTime { get; set; }
        public ProjectStatus Status { get; set; }
        public string UrlImage { get; set; }
        public string? ArchitectureInfo { get; set; }
        public List<TechnologyDto> Technologies { get; set; } = new();
        public List<ProjectMemberDto> Members { get; set; } = new();
    }
}
