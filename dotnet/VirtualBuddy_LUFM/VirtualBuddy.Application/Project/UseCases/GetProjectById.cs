using MapsterMapper;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Common;

namespace VirtualBuddy.Application.Project.UseCases
{
    public class GetProjectById
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetProjectById(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<GetProjectResponseDto> Execute(Guid id)
        {
            var project = await _repository.GetByIdAsync<Domain.Project.Project>(id);

            if (project == null)
            {
                throw new Exception($"Project with ID {id} not found.");
            }

            return _mapper.Map<GetProjectResponseDto>(project);
        }
    }
}
