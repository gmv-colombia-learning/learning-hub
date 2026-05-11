using VirtualBuddy.Domain.Common;

namespace VirtualBuddy.Domain.Project.Entities
{
    public class Technology : Entity
    {
        public string Name { get; private set; }
        
        // Relación Many-to-Many con Project
        private readonly List<Project> _projects = new();
        public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();

        private Technology() { } // Para EF Core

        public Technology(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Domain.Common.Exceptions.ValidationException("Technology name is required.");
            
            Name = name;
        }
    }
}
