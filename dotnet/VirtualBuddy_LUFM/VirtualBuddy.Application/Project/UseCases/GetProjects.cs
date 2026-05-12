using MapsterMapper;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Project.Specifications;

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
            var spec = new ProjectWithDetailsSpecification();
            var projects = await _repository.GetAllWithSpecAsync(spec);

            return _mapper.Map<ICollection<GetProjectResponseDto>>(projects);
        }
    }
}
