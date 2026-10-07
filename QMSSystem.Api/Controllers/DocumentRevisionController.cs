using Microsoft.AspNetCore.Mvc;
using QMSSystem.Api.Services;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Controllers
{
    [ApiController]
    [Route("api/document-revisions")]
    public class DocumentRevisionController : ControllerBase
    {
        private readonly DocumentRevisionService _revisionService;

        public DocumentRevisionController(
            DocumentRevisionService revisionService)
        {
            _revisionService = revisionService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadRevision(
            [FromForm] int documentId,
            [FromForm] string version,
            [FromForm] string uploadedBy,
            [FromForm] IFormFile file,
            [FromForm] string changeSummary)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var revision = new DocumentRevision
            {
                DocumentId = documentId,
                Version = int.Parse(version),
                ChangeSummary = changeSummary,
                UploadedBy = uploadedBy
            };

            var result =
                await _revisionService.UploadRevisionAsync(
                    revision,
                    file);

            if (result)
            {
                return Ok("Revision uploaded successfully.");
            }

            return StatusCode(
                500,
                "An error occurred while uploading the revision.");
        }

        [HttpGet("{documentId}")]
        public async Task<IActionResult> GetRevisions(
            int documentId)
        {
            var revisions =
                await _revisionService
                    .GetRevisionsByDocumentIdAsync(documentId);

            return Ok(revisions);
        }
    }
}