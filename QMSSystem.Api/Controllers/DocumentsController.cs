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

    [HttpPost]
    public async Task<IActionResult> CreateDocument(
        [FromBody] DocumentCreation document)
    {
        document.DocumentVersion = 0;
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
public async Task<IActionResult> DownloadCurrentFile(int id)
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

    return File(
        document.FileData,
        string.IsNullOrWhiteSpace(document.ContentType) ? "application/octet-stream" : document.ContentType,
        Path.GetFileName(document.FileName));
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

    // 1. Snapshot the CURRENT version into history
     _context.DocumentHistories.Add(new DocumentHistory
    {
        DocumentId      = doc.Id,
        DocumentNumber  = doc.DocumentNumber,
        Title           = doc.Title,
        Department      = doc.Department,
        DocumentVersion = doc.DocumentVersion,
        Status          = "Inactive",
        FileName        = doc.FileName,
        ContentType     = doc.ContentType,
        FileData        = doc.FileData,
        CreatedBy       = doc.CreatedBy,
        CreationOn      = doc.CreationOn,
        Comment         = doc.Comment,
        ArchivedOn      = DateTime.UtcNow,
        ArchivedBy      = 1
    });

    // 2. Read uploaded file
    using var ms = new MemoryStream();
    await file.CopyToAsync(ms);

    // 3. Update the live row + bump version
    doc.FileName        = file.FileName;
    doc.ContentType     = file.ContentType;
    doc.FileData        = ms.ToArray();
    doc.DocumentNumber = documentNumber.Trim();
    doc.Title           = title.Trim();
    doc.Department     = department.Trim();
    doc.DocumentVersion = doc.DocumentVersion + 1;
    doc.Comment         = comment;
    doc.Status          = "Pending";
    doc.DecisionStatus  = null;
    doc.DecisionBy      = null;
    doc.DecisionDate    = null;

    // EF Core saves the archive insert and active-document update in one transaction.
    await _context.SaveChangesAsync();

    return Ok(new { message = "Revision uploaded.", documentId = doc.Id, version = doc.DocumentVersion });
}
}