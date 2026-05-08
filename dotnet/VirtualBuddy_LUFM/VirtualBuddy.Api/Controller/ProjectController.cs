using Microsoft.AspNetCore.Mvc;
using VirtualBuddy.Application.DTOs.Response;
using VirtualBuddy.Application.Project;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace VirtualBuddy.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectFacade _projectFacade;

        public ProjectController(ProjectFacade projectFacade)
        {
            _projectFacade = projectFacade;
        }

        // GET: api/<ProjectController>
        [HttpGet]
        public async Task<ActionResult<ICollection<GetProjectResponseDto>>> Get()
        {
            var projects = await _projectFacade.GetProjects.Execute();
            return Ok(projects);
        }

        // GET api/<ProjectController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GetProjectResponseDto>> Get(Guid id)
        {
            var project = await _projectFacade.GetProjectById.Execute(id);
            return Ok(project);
        }

        // POST api/<ProjectController>
        [HttpPost]
        public async Task<ActionResult<GetProjectResponseDto>> Post([FromBody] Application.DTOs.Request.CreateProjectRequestDto request)
        {
            var project = await _projectFacade.CreateProject.Execute(request);
            return Ok(project);
        }

        // PUT api/<ProjectController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult<GetProjectResponseDto>> Put(Guid id, [FromBody] Application.DTOs.Request.UpdateProjectRequestDto request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID in route does not match ID in request body.");
            }

            var project = await _projectFacade.UpdateProject.Execute(request);
            return Ok(project);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<GetProjectResponseDto>> Patch(Guid id, [FromBody] Application.DTOs.Request.PatchProjectRequestDto request)
        {
            var project = await _projectFacade.PatchProject.Execute(id, request);
            return Ok(project);
        }

        // DELETE api/<ProjectController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _projectFacade.DeleteProject.Execute(id);
            return NoContent();
        }
    }
}
