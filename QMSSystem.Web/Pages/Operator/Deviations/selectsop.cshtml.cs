using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Operator.Deviations;

[AllowAnonymous]
public class SelectSopModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public List<Document> ActiveSops { get; private set; } = [];

    public void OnGet()
    {
        // The document API is not available yet; do not invent active SOP records.
        var activeSops = new List<Document>();
        var query = activeSops.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            query = query.Where(document =>
                document.DocumentNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                document.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                document.Department.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        ActiveSops = query
            .Where(document =>
                document.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                document.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            .OrderBy(document => document.DocumentNumber)
            .ToList();
    }
}