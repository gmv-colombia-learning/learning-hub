using VirtualBuddy.Application.Project.UseCases;

namespace VirtualBuddy.Application.Project
{
    public class ProjectFacade
    {
        public GetProjects GetProjects { get; }

        public ProjectFacade(GetProjects getProjects)
        {
            GetProjects = getProjects;
        }
    }
}
