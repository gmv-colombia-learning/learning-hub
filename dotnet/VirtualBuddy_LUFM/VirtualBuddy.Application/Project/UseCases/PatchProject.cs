using MapsterMapper;
using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Common;

namespace VirtualBuddy.Application.Project.UseCases
{
    public class PatchProject
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public PatchProject(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<GetProjectResponseDto> Execute(Guid id, PatchProjectRequestDto request)
        {
            var project = await _repository.GetByIdAsync<Domain.Project.Project>(id);

            if (project == null)
            {
                throw new Exception($"Project with ID {id} not found.");
            }

            // Aplicar cambios parciales validando reglas de dominio
            if (request.Name != null || request.Description != null || request.UrlImage != null || request.Acronym != null)
            {
                project.UpdateBasicInfo(
                    request.Name ?? project.Name,
                    request.Description ?? project.Description,
                    request.UrlImage ?? project.UrlImage,
                    request.Acronym ?? project.Acronym
                );
            }

            if (request.Status.HasValue && project.Status != request.Status.Value)
            {
                switch (request.Status.Value)
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

            return _mapper.Map<GetProjectResponseDto>(project);
        }
    }
}
