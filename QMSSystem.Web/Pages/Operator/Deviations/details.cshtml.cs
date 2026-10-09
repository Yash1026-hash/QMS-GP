using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QMSSystem.Web.Pages.Operator.Deviations;

public class DetailsModel : PageModel   
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public void OnGet()
    {
        // Frontend only for now.
        // The actual deviation details will be loaded from the backend later.
    }
}