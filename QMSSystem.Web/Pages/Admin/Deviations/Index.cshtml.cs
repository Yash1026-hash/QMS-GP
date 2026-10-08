using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;

namespace QMSSystem.Web.Pages.Admin.Deviations;

public class IndexModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? PriorityFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DecisionFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }

    public List<DeviationRequestDto> Deviations { get; set; } = [];

    public void OnGet()
    {
        // Ready for database/API repository integration
        var allDeviations = Deviations;

        TotalCount = allDeviations.Count;
        PendingCount = allDeviations.Count(d => d.Status == 0);
        ActiveCount = allDeviations.Count(d => d.Status == 1);
        InactiveCount = allDeviations.Count(d => d.Status == 2);
        ApprovedCount = allDeviations.Count(d => d.Decision == 1);
        RejectedCount = allDeviations.Count(d => d.Decision == 2);

        var query = allDeviations.AsEnumerable();

        if (StatusFilter.HasValue)
        {
            query = query.Where(d => d.Status == StatusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(PriorityFilter))
        {
            query = query.Where(d => d.Priority.Equals(PriorityFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (DecisionFilter.HasValue)
        {
            if (DecisionFilter.Value == -1)
            {
                query = query.Where(d => !d.Decision.HasValue);
            }
            else
            {
                query = query.Where(d => d.Decision == DecisionFilter.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var term = SearchQuery.Trim();
            query = query.Where(d =>
                d.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                d.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                d.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                d.CreatedBy.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        Deviations = query.OrderByDescending(d => d.CreatedDate).ToList();
    }
}
