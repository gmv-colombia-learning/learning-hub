using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VirtualBuddy.Application.AI.UseCases;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Application.Document.UseCases;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Enums;
using Xunit;

namespace VirtualBuddy.Test.Application
{
    public sealed class DocumentUseCasesTests
    {
        [Theory]
        [InlineData("guide.PDF", "application/octet-stream", "application/pdf")]
        [InlineData("guide.docx", "application/octet-stream", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
        [InlineData("guide.xlsx", "application/octet-stream", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("guide.xls", "application/octet-stream", "application/vnd.ms-excel")]
        [InlineData("guide.txt", "application/octet-stream", "text/plain")]
        [InlineData("guide.pdf", "application/pdf", "application/pdf")]
        [InlineData("guide.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
        [InlineData("guide.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
        [InlineData("guide.xls", "application/vnd.ms-excel", "application/vnd.ms-excel")]
        [InlineData("guide.txt", "text/plain", "text/plain")]
        public async Task UploadDocument_WithSupportedContentType_ShouldNormalizeAndIndex(
            string fileName,
            string declaredContentType,
            string expectedContentType)
        {
            var dependencies = CreateUploadDocument();
            using var stream = new MemoryStream([1, 2, 3]);

            var result = await dependencies.UseCase.ExecuteAsync(
                dependencies.Project.Id,
                stream,
                fileName,
                declaredContentType,
                stream.Length);

            result.Type.Should().Be(nameof(DocumentType.File));
            result.ContentType.Should().Be(expectedContentType);
            dependencies.Storage.Verify(
                item => item.UploadFileAsync(fileName, It.IsAny<Stream>(), expectedContentType),
                Times.Once);
            dependencies.Repository.Verify(
                item => item.AddAsync(It.Is<VirtualBuddy.Domain.Document.Document>(document =>
                    document.Type == DocumentType.File && document.ContentType == expectedContentType)),
                Times.Once);
            dependencies.Parser.Verify(
                item => item.ExtractTextAsync(It.IsAny<Stream>(), fileName),
                Times.Once);
            dependencies.KnowledgeBase.Verify(
                item => item.AddChunksAsync(
                    dependencies.Project.Id,
                    It.IsAny<Guid?>(),
                    It.IsAny<IEnumerable<(string Content, float[] Embedding, string? Metadata)>>()),
                Times.Once);
        }

        [Theory]
        [InlineData("document.bin")]
        [InlineData("document")]
        public async Task UploadDocument_WithUnsupportedOctetStream_ShouldNotIndex(string fileName)
        {
            var dependencies = CreateUploadDocument();
            using var stream = new MemoryStream([1, 2, 3]);

            var result = await dependencies.UseCase.ExecuteAsync(
                dependencies.Project.Id,
                stream,
                fileName,
                "application/octet-stream",
                stream.Length);

            result.Type.Should().Be(nameof(DocumentType.Unknown));
            result.ContentType.Should().Be("application/octet-stream");
            dependencies.Storage.Verify(
                item => item.UploadFileAsync(fileName, It.IsAny<Stream>(), "application/octet-stream"),
                Times.Once);
            dependencies.Parser.VerifyNoOtherCalls();
            dependencies.AI.VerifyNoOtherCalls();
            dependencies.KnowledgeBase.VerifyNoOtherCalls();
        }

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

        private static (
            UploadDocument UseCase,
            VirtualBuddy.Domain.Project.Project Project,
            Mock<IRepository> Repository,
            Mock<IFileStorageService> Storage,
            Mock<IDocumentParser> Parser,
            Mock<IAIService> AI,
            Mock<IKnowledgeBaseService> KnowledgeBase) CreateUploadDocument()
        {
            var project = new VirtualBuddy.Domain.Project.Project(
                "Test project",
                "A valid project description");
            var repository = new Mock<IRepository>();
            repository.Setup(item => item.GetByIdAsync<VirtualBuddy.Domain.Project.Project>(project.Id))
                .ReturnsAsync(project);
            repository.Setup(item => item.AddAsync(It.IsAny<VirtualBuddy.Domain.Document.Document>()))
                .ReturnsAsync((VirtualBuddy.Domain.Document.Document document) => document);

            var storage = new Mock<IFileStorageService>();
            storage.Setup(item => item.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync("documents/file");
            storage.Setup(item => item.GetSignedUrlAsync("documents/file", It.IsAny<int>()))
                .ReturnsAsync("https://example.test/documents/file");

            var parser = new Mock<IDocumentParser>();
            parser.Setup(item => item.ExtractTextAsync(It.IsAny<Stream>(), It.IsAny<string>()))
                .ReturnsAsync("Extracted document content");
            var ai = new Mock<IAIService>();
            ai.Setup(item => item.GetEmbeddingAsync(It.IsAny<string>())).ReturnsAsync(new float[768]);
            var knowledgeBase = new Mock<IKnowledgeBaseService>();
            knowledgeBase.Setup(item => item.AddChunksAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<IEnumerable<(string Content, float[] Embedding, string? Metadata)>>()))
                .Returns(Task.CompletedTask);

            var indexDocument = new IndexDocument(parser.Object, ai.Object, knowledgeBase.Object);
            var useCase = new UploadDocument(
                repository.Object,
                storage.Object,
                indexDocument,
                Mock.Of<ILogger<UploadDocument>>());

            return (useCase, project, repository, storage, parser, ai, knowledgeBase);
        }
    }
}
