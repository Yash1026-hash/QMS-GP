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
        .FirstOrDefaultAsync(d => d.Id == id);

    if (document == null)
    {
        return NotFound();
    }

    return Ok(document);
}
}