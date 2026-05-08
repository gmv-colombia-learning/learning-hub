using VirtualBuddy.Application.Project.UseCases;

namespace VirtualBuddy.Application.Project
{
    public class ProjectFacade
    {
        public GetProjects GetProjects { get; }
        public GetProjectById GetProjectById { get; }
        public CreateProject CreateProject { get; }
        public UpdateProject UpdateProject { get; }
        public PatchProject PatchProject { get; }
        public DeleteProject DeleteProject { get; }

        public ProjectFacade(GetProjects getProjects, 
                             GetProjectById getProjectById,
                             CreateProject createProject, 
                             UpdateProject updateProject, 
                             PatchProject patchProject,
                             DeleteProject deleteProject)
        {
            GetProjects = getProjects;
            GetProjectById = getProjectById;
            CreateProject = createProject;
            UpdateProject = updateProject;
            PatchProject = patchProject;
            DeleteProject = deleteProject;
        }
    }
}
