using VirtualBuddy.Infraestructure.data;

namespace VirtualBuddy.Infraestructure.Persistence.ProjectPersistence
{
    public class ProjectRepository : Repository
    {
        public ProjectRepository(BuddyDBContext context) : base(context)
        {
        }
    }
}
