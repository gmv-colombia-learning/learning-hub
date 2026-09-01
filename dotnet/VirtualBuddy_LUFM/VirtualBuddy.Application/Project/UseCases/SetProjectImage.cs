using MapsterMapper;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Exceptions;

namespace VirtualBuddy.Application.Project.UseCases
{
    public class SetProjectImage
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly ProjectImageService _projectImageService;

        public SetProjectImage(
            IRepository repository,
            IMapper mapper,
            ProjectImageService projectImageService)
        {
            _repository = repository;
            _mapper = mapper;
            _projectImageService = projectImageService;
        }

        public async Task<GetProjectResponseDto> Execute(
            Guid projectId,
            Stream imageStream,
            string fileName,
            string contentType,
            long fileSize)
        {
            var project = await _repository.GetByIdAsync<Domain.Project.Project>(projectId);
            if (project == null)
                throw new NotFoundException(nameof(Domain.Project.Project), projectId);

            var previousUrl = project.UrlImage;
            var uploadedUrl = await _projectImageService.UploadAsync(
                projectId,
                imageStream,
                fileName,
                contentType,
                fileSize);
            try
            {
                project.SetImageUrl(uploadedUrl);
                _repository.Update(project);
                await _repository.SaveChangesAsync();
            }
            catch
            {
                if (uploadedUrl != previousUrl)
                    await _projectImageService.DeleteIfManagedAsync(projectId, uploadedUrl);
                throw;
            }

            if (previousUrl != project.UrlImage)
                await _projectImageService.DeleteIfManagedAsync(projectId, previousUrl);

            return _mapper.Map<GetProjectResponseDto>(project);
        }
    }
}
