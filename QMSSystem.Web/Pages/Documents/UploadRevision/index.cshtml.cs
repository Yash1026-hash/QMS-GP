using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Documents;

public class UploadRevisionModel : PageModel
{
    private readonly UserDbContext _db;
    public UploadRevisionModel(UserDbContext db) => _db = db;

    public DocumentCreation Document { get; set; } = null!;

    [BindProperty] public string? Comment { get; set; }
    [BindProperty] public IFormFile? File { get; set; }

    // Load the current document to show info
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var doc = await _db.DocumentCreations.FindAsync(id);
        if (doc is null) return NotFound();

        Document = doc;
        return Page();
    }

    // Handle upload
    public async Task<IActionResult> OnPostAsync(int id)
    {
        var doc = await _db.DocumentCreations.FindAsync(id);
        if (doc is null) return NotFound();

        Document = doc;

        if (File is null || File.Length == 0)
        {
            ModelState.AddModelError("", "Please choose a file.");
            return Page();
        }

        // ── STEP 1: Snapshot the CURRENT version into history ──
        _db.DocumentHistories.Add(new DocumentHistory
        {
            DocumentId      = doc.Id,
            DocumentNumber  = doc.DocumentNumber,
            Title           = doc.Title,
            Department      = doc.Department,
            DocumentVersion = doc.DocumentVersion,
            Status          = doc.Status,
            FileName        = doc.FileName,
            ContentType     = doc.ContentType,
            FileData        = doc.FileData,
            CreatedBy       = doc.CreatedBy,
            CreationOn      = doc.CreationOn,
            Comment         = doc.Comment,
            ArchivedOn      = DateTime.UtcNow,
            ArchivedBy      = 1        // TODO: current logged-in user
        });

        // ── STEP 2: Read uploaded file into byte[] ──
        using var ms = new MemoryStream();
        await File.CopyToAsync(ms);

        // ── STEP 3: Update the LIVE row with new file + bump version ──
        doc.FileName        = File.FileName;
        doc.ContentType     = File.ContentType;
        doc.FileData        = ms.ToArray();
        doc.DocumentVersion = doc.DocumentVersion + 1;
        doc.Comment         = Comment;         // new revision's change note
        doc.Status          = "Draft";         // reset status for review flow
        doc.CreationOn      = DateTime.UtcNow;
        doc.CreatedBy       = 1;

        // ── STEP 4: Save both changes in one transaction ──
        await _db.SaveChangesAsync();

        return RedirectToPage("./Details", new { id = doc.Id });
    }
}