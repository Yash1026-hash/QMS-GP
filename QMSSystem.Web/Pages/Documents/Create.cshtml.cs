using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Documents;

public class CreateModel : PageModel
{
    [BindProperty]
    public DocumentCreation Document { get; set; } = new()
    {
        DocumentVersion = 1,
        Status = "Pending"
    };

    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (UploadFile != null && UploadFile.Length > 0)
        {
            Document.FileName = UploadFile.FileName;
            Document.ContentType = UploadFile.ContentType;

            using var memoryStream = new MemoryStream();
            await UploadFile.CopyToAsync(memoryStream);
            Document.FileData = memoryStream.ToArray();
        }

        Document.CreationOn = DateTime.UtcNow;

        // Mock confirmation until API/database persistence is wired
        IsSuccess = true;
        Message = $"Document '{Document.DocumentNumber}' created successfully (ready for database save).";

        return Page();
    }
}
