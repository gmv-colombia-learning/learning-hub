using MapsterMapper;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Common;

namespace VirtualBuddy.Application.Project.UseCases
{
    public class GetProjects
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetProjects(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task<ICollection<GetProjectResponseDto>> Execute()
        {
            var projects = await _repository.GetAllAsync<Domain.Project.Project>();

            return _mapper.Map<ICollection<GetProjectResponseDto>>(projects);
        }
    }
}
