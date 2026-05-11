using MapsterMapper;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Exceptions;
using VirtualBuddy.Domain.Project.Specifications;

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
            var spec = new ProjectWithDetailsSpecification(id);
            var project = await _repository.GetEntityWithSpecAsync(spec);

            if (project == null)
            {
                throw new NotFoundException(nameof(Domain.Project.Project), id);
            }

            return _mapper.Map<GetProjectResponseDto>(project);
        }
    }
}
