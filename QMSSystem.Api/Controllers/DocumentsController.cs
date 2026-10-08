using Microsoft.AspNetCore.Mvc;
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
}