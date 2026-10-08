using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.DTOs;

namespace QMSSystem.Web.Pages.Admin.ChangeRequests;

public class IndexModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ChangeTypeFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int PendingCount { get; set; }
    public int InactiveCount { get; set; }

    public List<OperatorChangeRequest> ChangeRequests { get; set; } = [];

    public void OnGet()
    {
        var allRequests = ChangeRequests;

        TotalCount = allRequests.Count;
        ActiveCount = allRequests.Count(c => c.Status.Equals("active", StringComparison.OrdinalIgnoreCase));
        PendingCount = allRequests.Count(c => c.Status.Equals("pending", StringComparison.OrdinalIgnoreCase));
        InactiveCount = allRequests.Count(c => c.Status.Equals("inactive", StringComparison.OrdinalIgnoreCase));

        var query = allRequests.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(StatusFilter))
        {
            query = query.Where(c => c.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(ChangeTypeFilter))
        {
            query = query.Where(c => c.ChangeType.Equals(ChangeTypeFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var term = SearchQuery.Trim();
            query = query.Where(c =>
                c.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                c.ChangeType.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        ChangeRequests = query.OrderByDescending(c => c.RequestedDate).ToList();
    }
}

