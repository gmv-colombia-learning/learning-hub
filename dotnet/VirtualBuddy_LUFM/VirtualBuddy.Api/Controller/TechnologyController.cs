using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtualBuddy.Application.Project;
using VirtualBuddy.Domain.Project.Entities;

namespace VirtualBuddy.Api.Controller
{
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class TechnologyController : ControllerBase
    {
        private readonly ProjectFacade _projectFacade;

        public TechnologyController(ProjectFacade projectFacade)
        {
            _projectFacade = projectFacade;
        }

        /// <summary>
        /// Lists all available technologies.
        /// </summary>
        /// <returns>The catalog of technologies.</returns>
        /// <response code="200">Returns the catalog of technologies.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<Technology>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<Technology>>> Get()
        {
            var technologies = await _projectFacade.GetTechnologies.ExecuteAsync();
            return Ok(technologies);
        }
    }
}
