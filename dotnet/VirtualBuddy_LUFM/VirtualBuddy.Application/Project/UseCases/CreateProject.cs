using MapsterMapper;
using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Common;

namespace VirtualBuddy.Application.Project.UseCases
{
    public class CreateProject
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public CreateProject(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<GetProjectResponseDto> Execute(CreateProjectRequestDto request)
        {
            var project = new Domain.Project.Project(
                request.Name,
                request.Description,
                request.UrlImage,
                request.Acronym,
                request.DevelopmentTime
            );

            await _repository.AddAsync(project);
            await _repository.SaveChangesAsync();

            return _mapper.Map<GetProjectResponseDto>(project);
        }
    }
}
