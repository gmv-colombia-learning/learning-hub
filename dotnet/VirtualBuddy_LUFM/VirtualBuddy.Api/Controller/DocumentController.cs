using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtualBuddy.Application.Document;
using VirtualBuddy.Application.DTOs.Response;

namespace VirtualBuddy.Api.Controller
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class DocumentController : ControllerBase
    {
        private readonly DocumentFacade _documentFacade;

        public DocumentController(DocumentFacade documentFacade)
        {
            _documentFacade = documentFacade;
        }

        /// <summary>
        /// Sube un documento asociado a un proyecto.
        /// </summary>
        [HttpPost("projects/{projectId}/documents")]
        public async Task<ActionResult<DocumentResponseDto>> UploadDocument(
            Guid projectId, 
            IFormFile file, 
            [FromForm] string? description)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var stream = file.OpenReadStream();
            var result = await _documentFacade.Upload.ExecuteAsync(
                projectId,
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                description
            );

            return CreatedAtAction(nameof(GetProjectDocuments), new { projectId }, result);
        }

        /// <summary>
        /// Obtiene todos los documentos asociados a un proyecto.
        /// </summary>
        [HttpGet("projects/{projectId}/documents")]
        public async Task<ActionResult<IEnumerable<DocumentResponseDto>>> GetProjectDocuments(Guid projectId)
        {
            var result = await _documentFacade.GetByProject.ExecuteAsync(projectId);
            return Ok(result);
        }

        /// <summary>
        /// Elimina un documento por su ID.
        /// </summary>
        [HttpDelete("documents/{id}")]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            await _documentFacade.Delete.ExecuteAsync(id);
            return NoContent();
        }
    }
}
