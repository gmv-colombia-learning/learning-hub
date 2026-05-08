using VirtualBuddy.Domain.Common.Enums;
using VirtualBuddy.Domain.Project.ValueObjects;

namespace VirtualBuddy.Domain.Project
{
    public class Project
    {
        public Guid Id { get; private set; }
        public ProjectName Name { get; private set; }
        public string? Acronym { get; private set; }
        public ProjectDescription Description { get; private set; }
        public DateTime DevelopmentTime { get; private set; }
        public ProjectStatus Status { get; private set; }
        public string UrlImage { get; private set; }
        public string? InformationAI { get; private set; }

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
                throw new InvalidOperationException("Cannot deactivate a completed project.");

            Status = ProjectStatus.Inactive;
        }

        public void Activate()
        {
            Status = ProjectStatus.Active;
        }

        public void MoveToReview()
        {
            if (Status != ProjectStatus.Active)
                throw new InvalidOperationException("Only active projects can be moved to review.");

            Status = ProjectStatus.Review;
        }

        public void Complete()
        {
            if (Status != ProjectStatus.Review && Status != ProjectStatus.Active)
                throw new InvalidOperationException("Project must be Active or in Review to be completed.");

            Status = ProjectStatus.Completed;
        }

        public void SetInformationAI(string information)
        {
            InformationAI = information;
        }
    }
}
