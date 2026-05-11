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

        public UpdateProject(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<GetProjectResponseDto> Execute(UpdateProjectRequestDto request)
        {
            var project = await _repository.GetByIdAsync<Domain.Project.Project>(request.Id);

            if (project == null)
            {
                throw new NotFoundException(nameof(Domain.Project.Project), request.Id);
            }

            // Actualización de info básica usando comportamiento del dominio
            project.UpdateBasicInfo(request.Name, request.Description, request.UrlImage, request.Acronym);

            // Manejo de transiciones de estado explícitas si el estado solicitado es diferente
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

            return _mapper.Map<GetProjectResponseDto>(project);
        }
    }
}
