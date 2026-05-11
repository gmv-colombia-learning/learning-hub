using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Application.Project.UseCases
{
    public class DeleteProject
    {
        private readonly IRepository _repository;

        public DeleteProject(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Execute(Guid id)
        {
            var project = await _repository.GetByIdAsync<Domain.Project.Project>(id);

            if (project == null)
            {
                throw new NotFoundException(nameof(Domain.Project.Project), id);
            }

            _repository.Delete(project);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
