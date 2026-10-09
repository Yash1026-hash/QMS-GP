using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.DTOs;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/documenthistory")]
public class DocumentHistoryController : ControllerBase
{
    private readonly UserDbContext _db;

    public DocumentHistoryController(UserDbContext db)
    {
        _db = db;
    }

    // GET: api/documenthistory  → all rows
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rows = await _db.DocumentHistories
            .OrderByDescending(h => h.Id)
            .ToListAsync();

        return Ok(rows);
    }

    // GET: api/documenthistory/5  → one row
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var row = await _db.DocumentHistories.FindAsync(id);
        if (row is null) return NotFound();
        return Ok(row);
    }

    // GET: api/documenthistory/document/5  → all history for document 5
    [HttpGet("document/{documentId}")]
    public async Task<IActionResult> GetByDocument(int documentId)
    {
        var rows = await _db.DocumentHistories
            .Where(h => h.DocumentId == documentId)
            .OrderByDescending(h => h.DocumentVersion)
            .Select(h => new DocumentHistoryDto
            {
                Id              = h.Id,
                DocumentId      = h.DocumentId,
                DocumentNumber  = h.DocumentNumber,
                Title           = h.Title,
                Department      = h.Department,
                DocumentVersion = h.DocumentVersion,
                Status          = h.Status,
                FileName        = h.FileName,
                ContentType     = h.ContentType,
                CreatedBy       = h.CreatedBy,
                CreationOn      = h.CreationOn,
                Comment         = h.Comment,
                ArchivedOn      = h.ArchivedOn,
                ArchivedBy      = h.ArchivedBy
            })
            .ToListAsync();

        return Ok(rows);
    }

    // POST: api/documenthistory  → insert one row
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DocumentHistoryDto dto)
    {
        var entity = new DocumentHistory
        {
            DocumentId      = dto.DocumentId,
            DocumentNumber  = dto.DocumentNumber,
            Title           = dto.Title,
            Department      = dto.Department,
            DocumentVersion = dto.DocumentVersion,
            Status          = dto.Status,
            FileName        = dto.FileName,
            ContentType     = dto.ContentType,
            FileData        = Array.Empty<byte>(),
            CreatedBy       = dto.CreatedBy,
            CreationOn      = dto.CreationOn,
            Comment         = dto.Comment,
            ArchivedOn      = DateTime.UtcNow,
            ArchivedBy      = dto.CreatedBy
        };

        _db.DocumentHistories.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

     
}