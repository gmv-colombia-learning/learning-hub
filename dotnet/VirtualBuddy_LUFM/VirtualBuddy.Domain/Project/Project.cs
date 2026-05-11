using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Enums;
using VirtualBuddy.Domain.Common.Exceptions;
using VirtualBuddy.Domain.Project.ValueObjects;
using VirtualBuddy.Domain.Project.Entities;

namespace VirtualBuddy.Domain.Project
{
    public class Project : Entity
    {
        public ProjectName Name { get; private set; }
        public string? Acronym { get; private set; }
        public ProjectDescription Description { get; private set; }
        public DateTime DevelopmentTime { get; private set; }
        public ProjectStatus Status { get; private set; }
        public string UrlImage { get; private set; }
        public string? InformationAI { get; private set; }
        public string? ArchitectureInfo { get; private set; } // IA Generated

        // Relaciones
        private readonly List<Technology> _technologies = new();
        public IReadOnlyCollection<Technology> Technologies => _technologies.AsReadOnly();

        private readonly List<ProjectMember> _members = new();
        public IReadOnlyCollection<ProjectMember> Members => _members.AsReadOnly();

        // Constructor para EF Core
        private Project() { }

        public Project(ProjectName name, ProjectDescription description, string urlImage, string? acronym = null, DateTime? developmentTime = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            UrlImage = urlImage;
            Acronym = acronym;
            DevelopmentTime = developmentTime ?? DateTime.UtcNow;
            Status = ProjectStatus.Active;
        }

        public void AddTechnology(Technology technology)
        {
            if (!_technologies.Any(t => t.Id == technology.Id))
                _technologies.Add(technology);
        }

        public void RemoveTechnology(Guid technologyId)
        {
            var tech = _technologies.FirstOrDefault(t => t.Id == technologyId);
            if (tech != null) _technologies.Remove(tech);
        }

        public void AddMember(ProjectMember member)
        {
            if (!_members.Any(m => m.UserId == member.UserId))
                _members.Add(member);
        }

        public void SetArchitectureInfo(string info)
        {
            ArchitectureInfo = info;
        }

        public void UpdateBasicInfo(ProjectName name, ProjectDescription description, string urlImage, string? acronym = null)
        {
            Name = name;
            Description = description;
            UrlImage = urlImage;
            Acronym = acronym;
        }

        public void Deactivate()
        {
            if (Status == ProjectStatus.Completed)
                throw new ConflictException("Cannot deactivate a completed project.");

            Status = ProjectStatus.Inactive;
        }

        public void Activate()
        {
            Status = ProjectStatus.Active;
        }

        public void MoveToReview()
        {
            if (Status != ProjectStatus.Active)
                throw new ConflictException("Only active projects can be moved to review.");

            Status = ProjectStatus.Review;
        }

        public void Complete()
        {
            if (Status != ProjectStatus.Review && Status != ProjectStatus.Active)
                throw new ConflictException("Project must be Active or in Review to be completed.");

            Status = ProjectStatus.Completed;
        }

        public void SetInformationAI(string information)
        {
            InformationAI = information;
        }
    }
}
