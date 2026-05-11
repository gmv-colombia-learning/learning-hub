using VirtualBuddy.Domain.Common;

namespace VirtualBuddy.Domain.Project.Specifications
{
    public class ProjectWithDetailsSpecification : BaseSpecification<Project>
    {
        public ProjectWithDetailsSpecification(Guid id) 
            : base(p => p.Id == id)
        {
            AddInclude(p => p.Technologies);
            AddInclude(p => p.Members);
        }

        public ProjectWithDetailsSpecification() : base()
        {
            AddInclude(p => p.Technologies);
            AddInclude(p => p.Members);
            AddOrderBy(p => p.Name.Value);
        }
    }
}
