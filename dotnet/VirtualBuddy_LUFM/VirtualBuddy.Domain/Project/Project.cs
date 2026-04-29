using VirtualBuddy.Domain.Common.Enums;

namespace VirtualBuddy.Domain.Project
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Acronym { get; set; }
        public string Description { get; set; }
        public DateTime DevelopmentTime { get; set; }
        public ProjectStatus Status { get; set; }
        public string UrlImage { get; set; }
        public string? InformationAI { get; set; }
    }
}
