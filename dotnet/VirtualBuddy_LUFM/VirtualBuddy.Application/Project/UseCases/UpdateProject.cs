using MapsterMapper;
using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Application.Project.UseCases
{
    public class UpdateProject
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly ProjectImageService _projectImageService;

        public UpdateProject(IRepository repository, IMapper mapper, ProjectImageService projectImageService)
        {
            _repository = repository;
            _mapper = mapper;
            _projectImageService = projectImageService;
        }

        public async Task<GetProjectResponseDto> Execute(UpdateProjectRequestDto request)
        {
            var project = await _repository.GetByIdAsync<Domain.Project.Project>(request.Id);

            if (project == null)
            {
                throw new NotFoundException(nameof(Domain.Project.Project), request.Id);
            }

            var previousUrl = project.UrlImage;
            project.UpdateBasicInfo(
                request.Name,
                request.Description,
                request.UrlImage ?? project.UrlImage,
                request.Acronym);

            if (project.Status != request.Status)
            {
                switch (request.Status)
                {
                    case Domain.Common.Enums.ProjectStatus.Active:
                        project.Activate();
                        break;
                    case Domain.Common.Enums.ProjectStatus.Inactive:
                        project.Deactivate();
                        break;
                    case Domain.Common.Enums.ProjectStatus.Review:
                        project.MoveToReview();
                        break;
                    case Domain.Common.Enums.ProjectStatus.Completed:
                        project.Complete();
                        break;
                }
            }

            _repository.Update(project);
            await _repository.SaveChangesAsync();

            if (previousUrl != project.UrlImage)
                await _projectImageService.DeleteIfManagedAsync(project.Id, previousUrl);

            return _mapper.Map<GetProjectResponseDto>(project);
        }
    }
}
