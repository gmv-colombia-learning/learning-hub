using VirtualBuddy.Domain.Common;

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
                throw new Exception($"Project with ID {id} not found.");
            }

            _repository.Delete(project);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
