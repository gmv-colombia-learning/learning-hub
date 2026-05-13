using VirtualBuddy.Application.Document.UseCases;
using VirtualBuddy.Application.DTOs.Response;

namespace VirtualBuddy.Application.Document
{
    public class DocumentFacade
    {
        public UploadDocument Upload { get; }
        public GetProjectDocuments GetByProject { get; }
        public DeleteDocument Delete { get; }

        public DocumentFacade(
            UploadDocument upload, 
            GetProjectDocuments getByProject, 
            DeleteDocument delete)
        {
            Upload = upload;
            GetByProject = getByProject;
            Delete = delete;
        }
    }
}
