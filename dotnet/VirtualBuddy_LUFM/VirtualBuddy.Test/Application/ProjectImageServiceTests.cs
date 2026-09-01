using FluentAssertions;
using MapsterMapper;
using Moq;
using VirtualBuddy.Application.Common.Interfaces;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Application.Project;
using VirtualBuddy.Application.Project.UseCases;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Exceptions;
using VirtualBuddy.Domain.Project;
using Xunit;

namespace VirtualBuddy.Test.Application
{
    public class ProjectImageServiceTests
    {
        private readonly Mock<IProjectImageStorageService> _storageMock = new();

        public static TheoryData<string, string, byte[]> ValidImages => new()
        {
            { "project.jpg", "image/jpeg", [0xFF, 0xD8, 0xFF, 0x00] },
            { "project.png", "image/png", [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A] },
            { "project.webp", "image/webp", [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50] }
        };

        [Theory]
        [MemberData(nameof(ValidImages))]
        public async Task UploadAsync_WithAllowedImage_ShouldUpload(
            string fileName,
            string contentType,
            byte[] content)
        {
            var projectId = Guid.NewGuid();
            var expectedUrl = $"https://storage/{projectId}/image";
            _storageMock
                .Setup(storage => storage.UploadAsync(projectId, It.IsAny<Stream>(), contentType))
                .ReturnsAsync(expectedUrl);
            var service = new ProjectImageService(_storageMock.Object);

            var result = await service.UploadAsync(
                projectId, new MemoryStream(content), fileName, contentType, content.Length);

            result.Should().Be(expectedUrl);
            _storageMock.Verify(
                storage => storage.UploadAsync(projectId, It.IsAny<Stream>(), contentType),
                Times.Once);
        }

        [Theory]
        [MemberData(nameof(ValidImages))]
        public async Task UploadAsync_WithOctetStream_ShouldDetectAndNormalizeContentType(
            string fileName,
            string expectedContentType,
            byte[] content)
        {
            var projectId = Guid.NewGuid();
            _storageMock
                .Setup(storage => storage.UploadAsync(projectId, It.IsAny<Stream>(), expectedContentType))
                .ReturnsAsync("https://storage/project-image");
            var service = new ProjectImageService(_storageMock.Object);

            await service.UploadAsync(
                projectId,
                new MemoryStream(content),
                fileName,
                "application/octet-stream",
                content.Length);

            _storageMock.Verify(
                storage => storage.UploadAsync(projectId, It.IsAny<Stream>(), expectedContentType),
                Times.Once);
        }

        [Fact]
        public async Task UploadAsync_WithOctetStreamAndInvalidExtension_ShouldRejectWithoutUpload()
        {
            var service = new ProjectImageService(_storageMock.Object);
            var content = new byte[] { 0xFF, 0xD8, 0xFF, 0x00 };

            var act = () => service.UploadAsync(
                Guid.NewGuid(),
                new MemoryStream(content),
                "project.exe",
                "application/octet-stream",
                content.Length);

            await act.Should().ThrowAsync<ValidationException>();
            _storageMock.Verify(
                storage => storage.UploadAsync(It.IsAny<Guid>(), It.IsAny<Stream>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadAsync_WithOctetStreamAndInvalidSignature_ShouldRejectWithoutUpload()
        {
            var service = new ProjectImageService(_storageMock.Object);
            var content = new byte[] { 0x00, 0x01, 0x02, 0x03 };

            var act = () => service.UploadAsync(
                Guid.NewGuid(),
                new MemoryStream(content),
                "project.jpg",
                "application/octet-stream",
                content.Length);

            await act.Should().ThrowAsync<ValidationException>();
            _storageMock.Verify(
                storage => storage.UploadAsync(It.IsAny<Guid>(), It.IsAny<Stream>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadAsync_WhenContentDoesNotMatchMimeType_ShouldRejectWithoutUpload()
        {
            var service = new ProjectImageService(_storageMock.Object);
            var invalidContent = new byte[] { 0x00, 0x01, 0x02 };

            var act = () => service.UploadAsync(
                Guid.NewGuid(), new MemoryStream(invalidContent), "project.jpg", "image/jpeg", invalidContent.Length);

            await act.Should().ThrowAsync<ValidationException>();
            _storageMock.Verify(
                storage => storage.UploadAsync(It.IsAny<Guid>(), It.IsAny<Stream>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadAsync_WhenFileExceedsLimit_ShouldRejectWithoutReadingOrUpload()
        {
            var service = new ProjectImageService(_storageMock.Object);

            var act = () => service.UploadAsync(
                Guid.NewGuid(), Stream.Null, "project.png", "image/png", ProjectImageService.MaxFileSize + 1);

            await act.Should().ThrowAsync<ValidationException>();
            _storageMock.Verify(
                storage => storage.UploadAsync(It.IsAny<Guid>(), It.IsAny<Stream>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task UploadAsync_WhenFileIsExactlyAtLimit_ShouldUpload()
        {
            var projectId = Guid.NewGuid();
            var content = new byte[ProjectImageService.MaxFileSize];
            new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }.CopyTo(content, 0);
            _storageMock
                .Setup(storage => storage.UploadAsync(projectId, It.IsAny<Stream>(), "image/png"))
                .ReturnsAsync("https://storage/project-image");
            var service = new ProjectImageService(_storageMock.Object);

            await service.UploadAsync(
                projectId, new MemoryStream(content), "project.png", "image/png", content.Length);

            _storageMock.Verify(
                storage => storage.UploadAsync(projectId, It.IsAny<Stream>(), "image/png"),
                Times.Once);
        }

        [Fact]
        public async Task UploadAsync_WhenStorageFails_ShouldReturnTemporaryServiceError()
        {
            var content = new byte[] { 0xFF, 0xD8, 0xFF, 0x00 };
            _storageMock
                .Setup(storage => storage.UploadAsync(It.IsAny<Guid>(), It.IsAny<Stream>(), "image/jpeg"))
                .ThrowsAsync(new HttpRequestException());
            var service = new ProjectImageService(_storageMock.Object);

            var act = () => service.UploadAsync(
                Guid.NewGuid(), new MemoryStream(content), "project.jpeg", "image/jpeg", content.Length);

            await act.Should().ThrowAsync<TemporaryServiceUnavailableException>();
        }

        [Fact]
        public async Task SetProjectImage_WhenProjectDoesNotExist_ShouldNotUpload()
        {
            var repository = new Mock<IRepository>();
            var mapper = new Mock<IMapper>();
            var projectId = Guid.NewGuid();
            repository.Setup(value => value.GetByIdAsync<Project>(projectId)).ReturnsAsync((Project?)null);
            var useCase = new SetProjectImage(
                repository.Object,
                mapper.Object,
                new ProjectImageService(_storageMock.Object));

            var act = () => useCase.Execute(
                projectId, Stream.Null, "project.png", "image/png", 1);

            await act.Should().ThrowAsync<NotFoundException>();
            _storageMock.Verify(
                storage => storage.UploadAsync(It.IsAny<Guid>(), It.IsAny<Stream>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task SetProjectImage_WithValidImage_ShouldPersistPublicUrl()
        {
            var repository = new Mock<IRepository>();
            var mapper = new Mock<IMapper>();
            var project = new Project("Project", "Description long enough");
            var content = new byte[] { 0xFF, 0xD8, 0xFF, 0x00 };
            var publicUrl = $"https://storage/{project.Id}/image";
            repository.Setup(value => value.GetByIdAsync<Project>(project.Id)).ReturnsAsync(project);
            _storageMock
                .Setup(storage => storage.UploadAsync(project.Id, It.IsAny<Stream>(), "image/jpeg"))
                .ReturnsAsync(publicUrl);
            mapper.Setup(value => value.Map<GetProjectResponseDto>(project))
                .Returns(() => new GetProjectResponseDto { Id = project.Id, UrlImage = project.UrlImage });
            var useCase = new SetProjectImage(
                repository.Object,
                mapper.Object,
                new ProjectImageService(_storageMock.Object));

            var result = await useCase.Execute(
                project.Id, new MemoryStream(content), "project.jpg", "image/jpeg", content.Length);

            result.UrlImage.Should().Be(publicUrl);
            repository.Verify(value => value.Update(project), Times.Once);
            repository.Verify(value => value.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SetProjectImage_WhenReplacingManagedImage_ShouldDeletePreviousObject()
        {
            var repository = new Mock<IRepository>();
            var mapper = new Mock<IMapper>();
            var project = new Project("Project", "Description long enough", "https://storage/old-image");
            var content = new byte[] { 0xFF, 0xD8, 0xFF, 0x00 };
            var publicUrl = "https://storage/new-image";
            repository.Setup(value => value.GetByIdAsync<Project>(project.Id)).ReturnsAsync(project);
            _storageMock
                .Setup(storage => storage.UploadAsync(project.Id, It.IsAny<Stream>(), "image/jpeg"))
                .ReturnsAsync(publicUrl);
            _storageMock
                .Setup(storage => storage.IsManagedUrl(project.Id, "https://storage/old-image"))
                .Returns(true);
            mapper.Setup(value => value.Map<GetProjectResponseDto>(project)).Returns(new GetProjectResponseDto());
            var useCase = new SetProjectImage(
                repository.Object,
                mapper.Object,
                new ProjectImageService(_storageMock.Object));

            await useCase.Execute(
                project.Id, new MemoryStream(content), "project.jpg", "image/jpeg", content.Length);

            _storageMock.Verify(
                storage => storage.DeleteAsync(project.Id, "https://storage/old-image"),
                Times.Once);
        }

    }
}
