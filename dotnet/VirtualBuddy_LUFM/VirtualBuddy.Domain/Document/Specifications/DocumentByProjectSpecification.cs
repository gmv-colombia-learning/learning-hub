using VirtualBuddy.Domain.Common;

namespace VirtualBuddy.Domain.Document.Specifications
{
    public class DocumentByProjectSpecification : BaseSpecification<Document>
    {
        public DocumentByProjectSpecification(Guid projectId) 
            : base(d => d.ProjectId == projectId)
        {
            AddOrderByDescending(d => d.UploadedAt);
        }
    }
}
