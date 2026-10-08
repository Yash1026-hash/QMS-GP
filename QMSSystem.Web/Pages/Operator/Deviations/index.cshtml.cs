using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;

namespace QMSSystem.Web.Pages.Operator.Deviations;

public class IndexModel : PageModel
{
    public List<DeviationRequestDto> Deviations { get; private set; } = new();

    public int TotalCount => Deviations.Count;

    public int PageSize { get; private set; } = 10;

    public int TotalPages =>
        TotalCount == 0
            ? 0
            : (int)Math.Ceiling((double)TotalCount / PageSize);

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Priority { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public void OnGet()
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        // Frontend only for now.
        // Real deviation data will be loaded after backend integration.
        Deviations = new List<DeviationRequestDto>();
    }

    public string GetStatusText(int status)
    {
        return status switch
        {
            0 => "Pending",
            1 => "Active",
            2 => "Inactive",
            _ => "Unknown"
        };
    }

    public string GetStatusClass(int status)
    {
        return status switch
        {
            0 => "status-pending",
            1 => "status-active",
            2 => "status-inactive",
            _ => "status-inactive"
        };
    }

    public string GetPriorityText(string? priority)
    {
        return string.IsNullOrWhiteSpace(priority)
            ? "-"
            : priority;
    }
}