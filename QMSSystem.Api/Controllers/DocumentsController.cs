using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly UserDbContext _context;

    public DocumentsController(UserDbContext context)
    {
        _context = context;
    }

    [HttpGet("check-number")]
    public async Task<IActionResult> CheckDocumentNumber(
        [FromQuery] string? documentNumber,
        [FromQuery] int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return Ok(new { exists = false });
        }

        var normalized = documentNumber.Trim().ToLower();
        var exists = await _context.DocumentCreations
            .AnyAsync(d => d.DocumentNumber.ToLower() == normalized && (!excludeId.HasValue || d.Id != excludeId.Value));

        return Ok(new { exists, documentNumber = documentNumber.Trim() });
    }

    [HttpPost]
    public async Task<IActionResult> CreateDocument(
        [FromBody] DocumentCreation document)
    {
        if (string.IsNullOrWhiteSpace(document.DocumentNumber))
        {
            return BadRequest(new { message = "Document number is required." });
        }

        document.DocumentNumber = document.DocumentNumber.Trim();
        var normalized = document.DocumentNumber.ToLower();

        var exists = await _context.DocumentCreations
            .AnyAsync(d => d.DocumentNumber.ToLower() == normalized);

        if (exists)
        {
            return BadRequest(new { message = $"Document number '{document.DocumentNumber}' already exists. Document number must be unique." });
        }

        if (string.IsNullOrWhiteSpace(document.Title))
        {
            return BadRequest(new { message = "Title is required." });
        }

        if (string.IsNullOrWhiteSpace(document.Department))
        {
            return BadRequest(new { message = "Department is required." });
        }

        document.DocumentVersion = 1;
        document.Status = "Pending";
        document.CreationOn = DateTime.UtcNow;

        _context.DocumentCreations.Add(document);

        await _context.SaveChangesAsync();

        return Ok(document);
    }


//get documents
    [HttpGet]
    public async Task<IActionResult> GetDocuments()
    {
        var documents = await _context.DocumentCreations
            .AsNoTracking()
            .ToListAsync();

        return Ok(documents);
    }

    //Get details of document

    [HttpGet("{id}")]
public async Task<IActionResult> GetDocument(int id)
{
    var document = await _context.DocumentCreations
        .AsNoTracking()
        .Where(d => d.Id == id)
        .Select(d => new
        {
            Document = d,
            CreatedByName = _context.Users
                .Where(user => user.UserId == d.CreatedBy)
                .Select(user => string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName)
                .FirstOrDefault()
        })
        .FirstOrDefaultAsync();

    if (document == null)
    {
        return NotFound();
    }

    return Ok(new
    {
        document.Document.Id,
        document.Document.DocumentNumber,
        document.Document.Title,
        document.Document.Department,
        document.Document.DocumentVersion,
        document.Document.Status,
        document.Document.FileName,
        document.Document.ContentType,
        document.Document.CreatedBy,
        document.CreatedByName,
        document.Document.CreationOn,
        document.Document.Comment,
        document.Document.DecisionBy,
        document.Document.DecisionDate,
        document.Document.Decision
    });
}

[HttpGet("{id}/current-file")]
public async Task<IActionResult> DownloadCurrentFile(int id, [FromQuery] bool inline = false)
{
    var document = await _context.DocumentCreations
        .AsNoTracking()
        .Where(item => item.Id == id)
        .Select(item => new
        {
            item.FileName,
            item.ContentType,
            item.FileData
        })
        .FirstOrDefaultAsync();

    if (document is null || document.FileData.Length == 0)
    {
        return NotFound(new { message = "The current document file was not found." });
    }

    var contentType = string.IsNullOrWhiteSpace(document.ContentType) ? "application/octet-stream" : document.ContentType;
    var fileName = Path.GetFileName(document.FileName);

    if (inline)
    {
        Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
        return File(document.FileData, contentType);
    }

    return File(
        document.FileData,
        contentType,
        fileName);
}

[HttpPost("{id}/upload-revision")]
public async Task<IActionResult> UploadRevision(
    int id,
    IFormFile file,
    [FromForm] string documentNumber,
    [FromForm] string title,
    [FromForm] string department,
    [FromForm] string? comment)
{
    var doc = await _context.DocumentCreations.FindAsync(id);
    if (doc is null) return NotFound(new { message = $"Document {id} not found." });
    if (file is null || file.Length == 0) return BadRequest(new { message = "File required." });
    if (string.IsNullOrWhiteSpace(documentNumber)
        || string.IsNullOrWhiteSpace(title)
        || string.IsNullOrWhiteSpace(department))
    {
        return BadRequest(new { message = "Document number, title, and department are required." });
    }

    var trimmedDocNumber = documentNumber.Trim();
    var normalizedDocNumber = trimmedDocNumber.ToLower();
    var duplicateExists = await _context.DocumentCreations
        .AnyAsync(d => d.Id != id && d.DocumentNumber.ToLower() == normalizedDocNumber);
    if (duplicateExists)
    {
        return BadRequest(new { message = $"Document number '{trimmedDocNumber}' is already in use by another document. Document number must be unique." });
    }

    // 1. Snapshot the CURRENT active version into history as PendingRevisionBackup if not already saved
    var existingBackup = await _context.DocumentHistories
        .FirstOrDefaultAsync(h => h.DocumentId == doc.Id && h.Status == "PendingRevisionBackup");

    int baseVersion = doc.DocumentVersion;
    if (existingBackup == null)
    {
        _context.DocumentHistories.Add(new DocumentHistory
        {
            DocumentId      = doc.Id,
            DocumentNumber  = doc.DocumentNumber,
            Title           = doc.Title,
            Department      = doc.Department,
            DocumentVersion = doc.DocumentVersion,
            Status          = "PendingRevisionBackup",
            FileName        = doc.FileName,
            ContentType     = doc.ContentType,
            FileData        = doc.FileData,
            CreatedBy       = doc.CreatedBy,
            CreationOn      = doc.CreationOn,
            Comment         = doc.Comment,
            ArchivedOn      = DateTime.UtcNow,
            ArchivedBy      = 1
        });
    }
    else
    {
        baseVersion = existingBackup.DocumentVersion;
    }

    // 2. Read uploaded file
    using var ms = new MemoryStream();
    await file.CopyToAsync(ms);

    // 3. Stage the live row for supervisor review + bump version
    doc.FileName        = file.FileName;
    doc.ContentType     = file.ContentType;
    doc.FileData        = ms.ToArray();
    doc.DocumentNumber  = trimmedDocNumber;
    doc.Title           = title.Trim();
    doc.Department      = department.Trim();
    doc.DocumentVersion = baseVersion + 1;
    doc.Comment         = comment;
    doc.Status          = "Pending";
    doc.DecisionStatus  = null;
    doc.DecisionBy      = null;
    doc.DecisionDate    = null;

    // EF Core saves the backup insert (if any) and live document staging in one transaction
    await _context.SaveChangesAsync();

    return Ok(new { message = "Revision uploaded successfully. Awaiting supervisor review.", documentId = doc.Id, version = doc.DocumentVersion });
}
}