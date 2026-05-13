using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Application.Project;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VirtualBuddy.Api.Controller
{
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectFacade _projectFacade;

        public ProjectController(ProjectFacade projectFacade)
        {
            _projectFacade = projectFacade;
        }

        /// <summary>
        /// Adds a technology to a project.
        /// </summary>
        /// <param name="id">The project unique identifier.</param>
        /// <param name="request">The technology ID to add.</param>
        /// <returns>Result of the operation</returns>
        /// <response code="200">Technology added successfully.</response>
        /// <response code="400">Technology already associated or request invalid.</response>
        /// <response code="404">Project or Technology not found.</response>
        [HttpPost("{id}/technologies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddTechnology(Guid id, [FromBody] VirtualBuddy.Application.DTOs.Request.AddTechnologyRequestDto request)
        {
            var result = await _projectFacade.AddTechnologyToProject.ExecuteAsync(id, request);
            if (result.Succeeded)
                return Ok();
            if (result.Error == "Project not found" || result.Error == "Technology not found")
                return NotFound(new ProblemDetails { Title = "Not Found", Detail = result.Error });
            return BadRequest(new ProblemDetails { Title = "Business Rule Violation", Detail = result.Error });
        }

        /// <summary>
        /// Retrieves all projects with their details.
        /// </summary>
        /// <returns>A collection of projects.</returns>
        /// <response code="200">Returns the list of projects.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ICollection<GetProjectResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ICollection<GetProjectResponseDto>>> Get()
        {
            var projects = await _projectFacade.GetProjects.Execute();
            return Ok(projects);
        }

        /// <summary>
        /// Retrieves a specific project by its ID.
        /// </summary>
        /// <param name="id">The project unique identifier.</param>
        /// <returns>The project details.</returns>
        /// <response code="200">Returns the project.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the project is not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GetProjectResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetProjectResponseDto>> Get(Guid id)
        {
            var project = await _projectFacade.GetProjectById.Execute(id);
            return Ok(project);
        }

        /// <summary>
        /// Creates a new project.
        /// </summary>
        /// <param name="request">The project creation data.</param>
        /// <returns>The created project details.</returns>
        /// <response code="200">Project created successfully.</response>
        /// <response code="400">If validation fails.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpPost]
        [ProducesResponseType(typeof(GetProjectResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GetProjectResponseDto>> Post([FromBody] Application.DTOs.Request.CreateProjectRequestDto request)
        {
            var project = await _projectFacade.CreateProject.Execute(request);
            return Ok(project);
        }

        /// <summary>
        /// Updates an existing project completely.
        /// </summary>
        /// <param name="id">The project unique identifier.</param>
        /// <param name="request">The project update data.</param>
        /// <returns>The updated project details.</returns>
        /// <response code="200">Project updated successfully.</response>
        /// <response code="400">If ID mismatch or validation fails.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the project is not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(GetProjectResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetProjectResponseDto>> Put(Guid id, [FromBody] Application.DTOs.Request.UpdateProjectRequestDto request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID in route does not match ID in request body.");
            }

            var project = await _projectFacade.UpdateProject.Execute(request);
            return Ok(project);
        }

        /// <summary>
        /// Partially updates an existing project.
        /// </summary>
        /// <param name="id">The project unique identifier.</param>
        /// <param name="request">The patch data.</param>
        /// <returns>The updated project details.</returns>
        /// <response code="200">Project patched successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the project is not found.</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(GetProjectResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetProjectResponseDto>> Patch(Guid id, [FromBody] Application.DTOs.Request.PatchProjectRequestDto request)
        {
            var project = await _projectFacade.PatchProject.Execute(id, request);
            return Ok(project);
        }

        /// <summary>
        /// Deletes a project.
        /// </summary>
        /// <param name="id">The project unique identifier.</param>
        /// <returns>No content.</returns>
        /// <response code="204">Project deleted successfully.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="404">If the project is not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _projectFacade.DeleteProject.Execute(id);
            return NoContent();
        }
    }
}
