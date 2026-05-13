using System;
using System.Linq;
using System.Threading.Tasks;
using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Domain.Project;
using VirtualBuddy.Domain.Project.Entities;
using VirtualBuddy.Domain.Project.Specifications;
using VirtualBuddy.Domain.Common;

namespace VirtualBuddy.Application.Project.UseCases
{
    public class AddTechnologyToProject
    {
        private readonly IRepository _repository;

        public AddTechnologyToProject(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> ExecuteAsync(Guid projectId, AddTechnologyRequestDto request)
        {
            // Load project with its technologies
            var spec = new ProjectWithDetailsSpecification(projectId);
            var project = await _repository.GetEntityWithSpecAsync<VirtualBuddy.Domain.Project.Project>(spec);
            if (project == null)
                return Result.Failure("Project not found");

            // Fetch Technology
            var technology = await _repository.GetByIdAsync<Technology>(request.TechnologyId);
            if (technology == null)
                return Result.Failure("Technology not found");

            // Check if already included
            if (project.Technologies.Any(t => t.Id == request.TechnologyId))
                return Result.Failure("Technology already associated with this project");

            // Add and persist
            project.AddTechnology(technology);
            await _repository.SaveChangesAsync();
            return Result.Success();
        }
    }

    public class Result
    {
        public bool Succeeded { get; }
        public string Error { get; }
        private Result(bool succeeded, string error) { Succeeded = succeeded; Error = error; }
        public static Result Success() => new Result(true, null);
        public static Result Failure(string error) => new Result(false, error);
    }
}
