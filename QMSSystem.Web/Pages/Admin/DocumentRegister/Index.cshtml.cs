using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Admin.DocumentRegister;

public class IndexModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? DepartmentFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int PendingCount { get; set; }
    public int InactiveCount { get; set; }

    public List<Document> Documents { get; set; } = [];

    public void OnGet()
    {
        // Ready for API / database integration; no demo data
        var allDocs = Documents;

        TotalCount = allDocs.Count;
        ActiveCount = allDocs.Count(d => d.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) || d.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase));
        PendingCount = allDocs.Count(d => d.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) || d.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase));
        InactiveCount = allDocs.Count(d => d.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase) || d.Status.Equals("Obsolete", StringComparison.OrdinalIgnoreCase));

        var query = allDocs.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(StatusFilter))
        {
            query = query.Where(d => d.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(DepartmentFilter))
        {
            query = query.Where(d => d.Department.Equals(DepartmentFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var term = SearchQuery.Trim();
            query = query.Where(d =>
                d.DocumentNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                d.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                d.Department.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        Documents = query.OrderByDescending(d => d.CreationOn).ToList();
    }
}

