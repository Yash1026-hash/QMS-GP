using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QMSSystem.Web.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.IsInRole("Admin"))
        {
            return RedirectToPage("/Admin/Index");
        }

        if (User.IsInRole("Supervisor"))
        {
            return RedirectToPage("/Supervisor/Index");
        }

        if (User.IsInRole("Operator"))
        {
            return RedirectToPage("/Operator/Index");
        }

        return Page();
    }
}
