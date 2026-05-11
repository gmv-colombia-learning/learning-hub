using VirtualBuddy.Domain.Common;

namespace VirtualBuddy.Domain.Project.Entities
{
    public class ProjectMember : Entity
    {
        public Guid ProjectId { get; private set; }
        public Guid UserId { get; private set; } // Referencia a Identity/Azure AD
        public string Role { get; private set; }
        public string FullName { get; private set; } // Caché del nombre para evitar joins constantes con Identity

        private ProjectMember() { } // Para EF Core

        public ProjectMember(Guid userId, string fullName, string role)
        {
            if (userId == Guid.Empty)
                throw new Domain.Common.Exceptions.ValidationException("User ID is required.");
            
            if (string.IsNullOrWhiteSpace(fullName))
                throw new Domain.Common.Exceptions.ValidationException("Full name is required.");

            if (string.IsNullOrWhiteSpace(role))
                throw new Domain.Common.Exceptions.ValidationException("Role is required.");

            UserId = userId;
            FullName = fullName;
            Role = role;
        }
    }
}
