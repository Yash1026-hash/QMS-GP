using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var createdBy))
        {
            return Unauthorized();
        }

        document.DocumentVersion = 0;
        document.Status = "Pending";
        document.CreatedBy = createdBy;
        document.CreationOn = DateTime.UtcNow;

        await using var transaction = await _context.Database.BeginTransactionAsync();
        _context.DocumentCreations.Add(document);
        await _context.SaveChangesAsync();

        _context.DocumentEvents.Add(new DocumentEvent
        {
            DocumentId = document.Id,
            EventType = "Created",
            ActorUserId = createdBy,
            EventOn = document.CreationOn,
            Comment = document.Comment
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        document.CreatedByName = await _context.Users
            .Where(user => user.UserId == document.CreatedBy)
            .Select(user => user.FullName)
            .FirstOrDefaultAsync();
        return Ok(document);
    }


    // Get documents
    [HttpGet]
    public async Task<IActionResult> GetDocuments()
    {
        var documents = await _context.DocumentCreations
            .AsNoTracking()
            .ToListAsync();

        var userIds = documents.Select(document => document.CreatedBy).Distinct().ToArray();
        var creatorNames = await _context.Users
            .Where(user => userIds.Contains(user.UserId))
            .ToDictionaryAsync(user => user.UserId, user => user.FullName);

        foreach (var document in documents)
        {
            document.CreatedByName = creatorNames.GetValueOrDefault(document.CreatedBy);
        }

        return Ok(documents);
    }


    // Get details of a document
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(int id)
    {
        var document = await _context.DocumentCreations
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (document == null)
        {
            return NotFound();
        }

        document.CreatedByName = await _context.Users
            .Where(user => user.UserId == document.CreatedBy)
            .Select(user => user.FullName)
            .FirstOrDefaultAsync();

        return Ok(document);
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

        _context.DocumentHistories.Add(new DocumentHistory
        {
            DocumentId = doc.Id,
            DocumentNumber = doc.DocumentNumber,
            Title = doc.Title,
            Department = doc.Department,
            DocumentVersion = doc.DocumentVersion,
            Status = "Inactive",
            FileName = doc.FileName,
            ContentType = doc.ContentType,
            FileData = doc.FileData,
            CreatedBy = doc.CreatedBy,
            CreationOn = doc.CreationOn,
            Comment = doc.Comment,
            ArchivedOn = DateTime.UtcNow,
            ArchivedBy = 1
        });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        doc.FileName = file.FileName;
        doc.ContentType = file.ContentType;
        doc.FileData = ms.ToArray();
        doc.DocumentNumber = documentNumber.Trim();
        doc.Title = title.Trim();
        doc.Department = department.Trim();
        doc.DocumentVersion++;
        doc.Comment = comment;
        doc.Status = "Pending";
        doc.DecisionStatus = null;
        doc.DecisionBy = null;
        doc.DecisionDate = null;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Revision uploaded.", documentId = doc.Id, version = doc.DocumentVersion });
    }

    [HttpGet("{id:int}/events")]
    public async Task<IActionResult> GetDocumentEvents(int id)
    {
        var documentExists = await _context.DocumentCreations
            .AnyAsync(document => document.Id == id);
        if (!documentExists)
        {
            return NotFound();
        }

        var events = await _context.DocumentEvents
            .AsNoTracking()
            .Where(documentEvent => documentEvent.DocumentId == id)
            .OrderBy(documentEvent => documentEvent.EventOn)
            .ToListAsync();

        var actorIds = events.Select(documentEvent => documentEvent.ActorUserId).Distinct().ToArray();
        var actorNames = await _context.Users
            .Where(user => actorIds.Contains(user.UserId))
            .ToDictionaryAsync(user => user.UserId, user => user.FullName);

        foreach (var documentEvent in events)
        {
            documentEvent.ActorName = actorNames.GetValueOrDefault(documentEvent.ActorUserId);
        }

        return Ok(events);
    }

    [Authorize(Policy = "DocumentReviewer")]
    [HttpPost("{id:int}/decision")]
    public async Task<IActionResult> RecordDecision(
        int id,
        [FromBody] DocumentDecisionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Comment))
        {
            ModelState.AddModelError(nameof(request.Comment), "A review comment is required.");
            return ValidationProblem(ModelState);
        }

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var actorUserId))
        {
            return Unauthorized();
        }

        var document = await _context.DocumentCreations
            .FirstOrDefaultAsync(item => item.Id == id);
        if (document is null)
        {
            return NotFound();
        }

        if (!string.Equals(document.Status, "Pending", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(document.Status, "Returned", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { message = "Only pending or returned documents can be reviewed." });
        }

        var (eventType, status) = request.Decision.ToUpperInvariant() switch
        {
            "APPROVE" => ("Approved", "Active"),
            "RETURN" => ("Returned", "Returned"),
            "REJECT" => ("Rejected", "Rejected"),
            _ => (string.Empty, string.Empty)
        };

        if (eventType.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Decision), "Choose approve, return, or reject.");
            return ValidationProblem(ModelState);
        }

        document.Status = status;
        _context.DocumentEvents.Add(new DocumentEvent
        {
            DocumentId = document.Id,
            EventType = eventType,
            ActorUserId = actorUserId,
            EventOn = DateTime.UtcNow,
            Comment = request.Comment.Trim()
        });

        await _context.SaveChangesAsync();
        return Ok(document);
    }
}