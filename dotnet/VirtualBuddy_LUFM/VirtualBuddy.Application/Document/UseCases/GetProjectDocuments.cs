using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Document.Specifications;

namespace VirtualBuddy.Application.Document.UseCases
{
    public class GetProjectDocuments
    {
        private readonly IRepository _repository;

        public GetProjectDocuments(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<DocumentResponseDto>> ExecuteAsync(Guid projectId)
        {
            var spec = new DocumentByProjectSpecification(projectId);
            var documents = await _repository.GetAllWithSpecAsync(spec);

            return documents.Select(d => new DocumentResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                Size = d.Size,
                UploadedAt = d.UploadedAt,
                Type = d.Type.ToString(),
                ProjectId = d.ProjectId,
                PublicUrl = d.PublicUrl,
                ContentType = d.ContentType
            });
        }
    }
}
