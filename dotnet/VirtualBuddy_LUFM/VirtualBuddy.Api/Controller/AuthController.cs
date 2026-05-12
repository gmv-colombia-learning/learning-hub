using Microsoft.AspNetCore.Mvc;
using VirtualBuddy.Application.Auth;
using VirtualBuddy.Application.DTOs.Request;
using VirtualBuddy.Application.DTOs.Response;

namespace VirtualBuddy.Api.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthFacade _authFacade;

        public AuthController(AuthFacade authFacade)
        {
            _authFacade = authFacade;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <param name="request">The login credentials.</param>
        /// <returns>Auth response with token.</returns>
        /// <response code="200">Returns the JWT token and user details.</response>
        /// <response code="401">If credentials are invalid.</response>
        /// <response code="404">If the user is not found.</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var response = await _authFacade.Login.Execute(request);
            return Ok(response);
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">The registration details.</param>
        /// <returns>Auth response with token.</returns>
        /// <response code="200">User created successfully and returns token.</response>
        /// <response code="400">If validation fails.</response>
        /// <response code="409">If the user already exists.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            var response = await _authFacade.Register.Execute(request);
            return Ok(response);
        }
    }
}
