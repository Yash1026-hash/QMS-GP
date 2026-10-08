using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.DTOs;

namespace QMSSystem.Web.Pages.Admin.Deviations;

public class DetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public DeviationRequestDto? Deviation { get; set; }

    public List<DeviationReportDto> Reports { get; set; } = [];

    public List<OperatorChangeRequest> LinkedChangeRequests { get; set; } = [];

    public IActionResult OnGet()
    {
        // Ready for database/API repository integration
        return Page();
    }
}
