using FluentAssertions;
using Moq;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Application.Document.UseCases;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Enums;
using Xunit;

namespace VirtualBuddy.Test.Application
{
    public sealed class DocumentUseCasesTests
    {
        [Fact]
        public async Task DeleteDocument_ShouldDeleteStorageChunksAndEntity()
        {
            var document = VirtualBuddy.Domain.Document.Document.Create(
                "document.txt",
                "1KB",
                DocumentType.File,
                Guid.NewGuid(),
                "documents/document.txt",
                "https://example.test/document.txt",
                "text/plain");
            var repository = new Mock<IRepository>();
            repository.Setup(item => item.GetByIdAsync<VirtualBuddy.Domain.Document.Document>(document.Id))
                .ReturnsAsync(document);
            var storage = new Mock<IFileStorageService>();
            storage.Setup(item => item.DeleteFileAsync(document.StoragePath)).ReturnsAsync(true);
            var knowledgeBase = new Mock<IKnowledgeBaseService>();
            repository.Setup(item => item.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
                .Returns((Func<Task> operation) => operation());
            var useCase = new DeleteDocument(repository.Object, storage.Object, knowledgeBase.Object);

            await useCase.ExecuteAsync(document.Id);

            storage.Verify(item => item.DeleteFileAsync(document.StoragePath), Times.Once);
            knowledgeBase.Verify(item => item.DeleteChunksByDocumentIdAsync(document.Id), Times.Once);
            repository.Verify(item => item.Delete(document), Times.Once);
            repository.Verify(item => item.SaveChangesAsync(), Times.Once);
            repository.Verify(item => item.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()), Times.Once);
        }

        [Fact]
        public async Task DeleteDocument_WhenDocumentDoesNotExist_ShouldNotDeleteDependencies()
        {
            var documentId = Guid.NewGuid();
            var repository = new Mock<IRepository>();
            repository.Setup(item => item.GetByIdAsync<VirtualBuddy.Domain.Document.Document>(documentId))
                .ReturnsAsync((VirtualBuddy.Domain.Document.Document?)null);
            var storage = new Mock<IFileStorageService>();
            var knowledgeBase = new Mock<IKnowledgeBaseService>();
            var useCase = new DeleteDocument(repository.Object, storage.Object, knowledgeBase.Object);

            var action = () => useCase.ExecuteAsync(documentId);

            await action.Should().ThrowAsync<KeyNotFoundException>();
            storage.VerifyNoOtherCalls();
            knowledgeBase.VerifyNoOtherCalls();
        }
    }
}
