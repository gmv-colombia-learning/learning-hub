using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Document;

namespace VirtualBuddy.Application.Document.UseCases
{
    public class DeleteDocument
    {
        private readonly IRepository _repository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IKnowledgeBaseService _knowledgeBaseService;

        public DeleteDocument(
            IRepository repository,
            IFileStorageService fileStorageService,
            IKnowledgeBaseService knowledgeBaseService)
        {
            _repository = repository;
            _fileStorageService = fileStorageService;
            _knowledgeBaseService = knowledgeBaseService;
        }

        public async Task ExecuteAsync(Guid documentId)
        {
            var document = await _repository.GetByIdAsync<VirtualBuddy.Domain.Document.Document>(documentId);
            if (document == null)
                throw new KeyNotFoundException($"Document with ID {documentId} not found.");

            // 1. Borrar del storage
            await _fileStorageService.DeleteFileAsync(document.StoragePath);

            // 2. Borrar el indice asociado
            await _repository.ExecuteInTransactionAsync(async () =>
            {
                await _knowledgeBaseService.DeleteChunksByDocumentIdAsync(document.Id);

                // 3. Borrar de la base de datos
                _repository.Delete(document);
                await _repository.SaveChangesAsync();
            });
        }
    }
}
