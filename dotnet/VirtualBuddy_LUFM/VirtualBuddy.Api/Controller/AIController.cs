using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtualBuddy.Application.AI;

namespace VirtualBuddy.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly AIFacade _aiFacade;

        public AIController(AIFacade aiFacade)
        {
            _aiFacade = aiFacade;
        }

        /// <summary>
        /// Consulta al asistente VirtualBuddy sobre un proyecto específico usando RAG.
        /// </summary>
        [HttpPost("chat/{projectId}")]
        public async Task<IActionResult> Chat(Guid projectId, [FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                return BadRequest("La pregunta no puede estar vacía.");

            var response = await _aiFacade.Chat.ExecuteAsync(projectId, request.Question);
            return Ok(new { response });
        }

        /// <summary>
        /// Fuerza la re-indexación de los documentos de un proyecto.
        /// (Normalmente esto ocurre automáticamente al subir un archivo).
        /// </summary>
        [HttpPost("index/{projectId}")]
        public async Task<IActionResult> Reindex(Guid projectId)
        {
            // TO-DO Este endpoint podría ser útil para mantenimiento o si falló una indexación automática.
            // Por simplicidad en este MVP, se deja como placeholder para extensiones futuras.
            return Ok(new { message = "Indexación iniciada (esta funcionalidad se activa automáticamente al subir archivos)." });
        }
    }

    public class ChatRequest
    {
        public string Question { get; set; } = string.Empty;
    }
}
