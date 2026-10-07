using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.API.Data;
using QMSSystem.Shared.Models;
using QMSSystem.Shared.Workflow;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DocumentsController(ApplicationDbContext context)
    {
        _context = context;
    }


    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateDocument(
        [FromForm] string documentNumber,
        [FromForm] string title,
        [FromForm] string department,
        [FromForm] int createdBy,
        [FromForm] string? comment,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentNumber) ||
            string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(department) ||
            createdBy <= 0)
        {
            return BadRequest("Invalid document details.");
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("Please select a document file.");
        }

        var fileName = Path.GetFileName(file.FileName);

        var extension = Path.GetExtension(fileName);

        if (!new[] { ".pdf", ".doc", ".docx" }
            .Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest("Only PDF, DOC and DOCX files are allowed.");
        }

        if (await _context.Documents.AnyAsync(
                x => x.DocumentNumber == documentNumber.Trim(),
                cancellationToken))
        {
            return Conflict("A document with this number already exists.");
        }

        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream, cancellationToken);

        var document = new Document
        {
            DocumentNumber = documentNumber.Trim(),
            Title = title.Trim(),
            Department = department.Trim(),

            CurrentVersion = 1,
            Status = DocumentStatuses.Draft,

            CreatedBy = createdBy,
            CreationTime = DateTime.UtcNow,

            FileName = fileName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType,

            FileData = stream.ToArray(),

            Comment = comment
        };

        _context.Documents.Add(document);

        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetDocumentById),
            new { id = document.Id },
            new { document.Id });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDocumentById(int id, CancellationToken cancellationToken)
    {
        var document = await _context.Documents.FindAsync([id], cancellationToken);
        if (document is null)
        {
            return NotFound();
        }

        return File(document.FileData, document.ContentType, document.FileName);
    }
}