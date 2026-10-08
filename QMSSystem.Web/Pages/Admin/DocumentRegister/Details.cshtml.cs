using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Admin.DocumentRegister;

public class DetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Document? Document { get; set; }

    public IActionResult OnGet()
    {
        // Ready for database/API retrieval
        return Page();
    }
}

