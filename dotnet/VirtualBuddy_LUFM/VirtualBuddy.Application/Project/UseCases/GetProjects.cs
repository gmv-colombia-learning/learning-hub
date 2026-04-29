using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Common;

namespace VirtualBuddy.Application.Project.UseCases
{
    public class GetProjects
    {
        private readonly IRepository _repository;

        public GetProjects(IRepository repository)
        {
            _repository = repository;
        }
        public async Task<ICollection<GetProjectResponseDto>> Execute()
        {

            //VAMOS AQUI OJO
            var projects = await _repository.GetAllAsync<Domain.Project.Project>();

            return null;
        }
    }
}
