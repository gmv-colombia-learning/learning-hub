using FluentAssertions;
using MapsterMapper;
using Moq;
using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Application.Project.UseCases;
using VirtualBuddy.Domain.Common;
using VirtualBuddy.Domain.Common.Exceptions;
using VirtualBuddy.Domain.Project;
using Xunit;

namespace VirtualBuddy.Test.Application
{
    public class ProjectUseCasesTests
    {
        private readonly Mock<IRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetProjects _getProjectsUseCase;
        private readonly GetProjectById _getProjectByIdUseCase;
        private readonly CreateProject _createProjectUseCase;
        private readonly UpdateProject _updateProjectUseCase;
        private readonly PatchProject _patchProjectUseCase;
        private readonly DeleteProject _deleteProjectUseCase;

        public ProjectUseCasesTests()
        {
            _repositoryMock = new Mock<IRepository>();
            _mapperMock = new Mock<IMapper>();
            _getProjectsUseCase = new GetProjects(_repositoryMock.Object, _mapperMock.Object);
            _getProjectByIdUseCase = new GetProjectById(_repositoryMock.Object, _mapperMock.Object);
            _createProjectUseCase = new CreateProject(_repositoryMock.Object, _mapperMock.Object);
            _updateProjectUseCase = new UpdateProject(_repositoryMock.Object, _mapperMock.Object);
            _patchProjectUseCase = new PatchProject(_repositoryMock.Object, _mapperMock.Object);
            _deleteProjectUseCase = new DeleteProject(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetProjectById_WhenProjectExists_ShouldReturnDto()
        {
            // Arrange
            var existingProject = new Project("Test Project", "Description long enough", "url");
            var projectId = existingProject.Id;
            var responseDto = new GetProjectResponseDto { Id = projectId, Name = "Test Project" };

            _repositoryMock.Setup(r => r.GetByIdAsync<Project>(projectId)).ReturnsAsync(existingProject);
            _mapperMock.Setup(m => m.Map<GetProjectResponseDto>(existingProject)).Returns(responseDto);

            // Act
            var result = await _getProjectByIdUseCase.Execute(projectId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(projectId);
            result.Name.Should().Be("Test Project");
            _repositoryMock.Verify(r => r.GetByIdAsync<Project>(projectId), Times.Once);
        }

        [Fact]
        public async Task GetProjects_ShouldReturnListOfDtos()
        {
            // Arrange
            var project = new Project("Test Project", "Description long enough", "url");
            var projects = new List<Project> { project };
            var dtos = new List<GetProjectResponseDto> { new GetProjectResponseDto { Id = project.Id, Name = "Test Project" } };

            _repositoryMock.Setup(r => r.GetAllAsync<Project>()).ReturnsAsync(projects);
            _mapperMock.Setup(m => m.Map<ICollection<GetProjectResponseDto>>(projects)).Returns(dtos);

            // Act
            var result = await _getProjectsUseCase.Execute();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Test Project");
            _repositoryMock.Verify(r => r.GetAllAsync<Project>(), Times.Once);
        }

        [Fact]
        public async Task CreateProject_ShouldReturnCreatedDto()
        {
            // Arrange
            var request = new CreateProjectRequestDto { Name = "New Project", Description = "Description long enough", UrlImage = "url" };
            var responseDto = new GetProjectResponseDto { Name = "New Project" };

            _mapperMock.Setup(m => m.Map<GetProjectResponseDto>(It.IsAny<Project>())).Returns(responseDto);

            // Act
            var result = await _createProjectUseCase.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New Project");
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateProject_WhenProjectExists_ShouldReturnUpdatedDto()
        {
            // Arrange
            var existingProject = new Project("Old Project", "Description long enough", "url");
            var projectId = existingProject.Id;
            var request = new UpdateProjectRequestDto { Id = projectId, Name = "Updated Project", Description = "Updated description long enough", UrlImage = "url", Status = Domain.Common.Enums.ProjectStatus.Active };
            var responseDto = new GetProjectResponseDto { Id = projectId, Name = "Updated Project" };

            _repositoryMock.Setup(r => r.GetByIdAsync<Project>(projectId)).ReturnsAsync(existingProject);
            _mapperMock.Setup(m => m.Map<GetProjectResponseDto>(existingProject)).Returns(responseDto);

            // Act
            var result = await _updateProjectUseCase.Execute(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Project");
            _repositoryMock.Verify(r => r.Update(existingProject), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task PatchProject_WhenProjectExists_ShouldReturnUpdatedDto()
        {
            // Arrange
            var existingProject = new Project("Old Project", "Description long enough", "url");
            var projectId = existingProject.Id;
            var request = new PatchProjectRequestDto { Name = "Patched Project" };
            var responseDto = new GetProjectResponseDto { Id = projectId, Name = "Patched Project" };

            _repositoryMock.Setup(r => r.GetByIdAsync<Project>(projectId)).ReturnsAsync(existingProject);
            _mapperMock.Setup(m => m.Map<GetProjectResponseDto>(existingProject)).Returns(responseDto);

            // Act
            var result = await _patchProjectUseCase.Execute(projectId, request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Patched Project");
            _repositoryMock.Verify(r => r.Update(existingProject), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteProject_WhenProjectExists_ShouldReturnTrue()
        {
            // Arrange
            var existingProject = new Project("Test", "Description long enough", "url");
            var projectId = existingProject.Id;

            _repositoryMock.Setup(r => r.GetByIdAsync<Project>(projectId)).ReturnsAsync(existingProject);

            // Act
            var result = await _deleteProjectUseCase.Execute(projectId);

            // Assert
            result.Should().BeTrue();
            _repositoryMock.Verify(r => r.Delete(existingProject), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }


        [Fact]
        public async Task UpdateProject_WhenProjectDoesNotExist_ShouldThrowNotFoundException()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var request = new UpdateProjectRequestDto { Id = projectId };
            _repositoryMock.Setup(r => r.GetByIdAsync<Project>(projectId)).ReturnsAsync((Project)null);

            // Act
            var act = () => _updateProjectUseCase.Execute(request);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
