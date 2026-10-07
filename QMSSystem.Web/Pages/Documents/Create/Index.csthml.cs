using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;

namespace QMS.Pages.Documents
{
    public class CreateModel : PageModel
    {
        [BindProperty]
        public Document Document { get; set; } = new();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                return Page();
            }

            // API call will be added later

            return RedirectToPage("/Index");
        }
    }
}