using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Application.Project.UseCases
{
    public class DeleteProject
    {
        private readonly IRepository _repository;
        private readonly ProjectImageService _projectImageService;

        public DeleteProject(IRepository repository, ProjectImageService projectImageService)
        {
            _repository = repository;
            _projectImageService = projectImageService;
        }

        public async Task<bool> Execute(Guid id)
        {
            var project = await _repository.GetByIdAsync<Domain.Project.Project>(id);

            if (project == null)
            {
                throw new NotFoundException(nameof(Domain.Project.Project), id);
            }

            await _projectImageService.DeleteIfManagedAsync(project.Id, project.UrlImage);
            _repository.Delete(project);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
