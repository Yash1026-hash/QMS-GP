using Microsoft.AspNetCore.Mvc;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/supervisor")]
public class SupervisorController : ControllerBase
{
    [HttpGet("modules")]
    public IActionResult GetModules()
    {
        var modules = new[]
        {
            new
            {
                Name = "Deviation Review",
                Description = "Review deviations",
                Route = "/Supervisor/DeviationReview"
            },

            new
            {
                Name = "Change Request Approval",
                Description = "Review and approve change requests",
                Route = "/Supervisor/ChangeRequestApproval"
            },

            new
            {
                Name = "Deviation Review Report",
                Description = "View deviation review reports",
                Route = "/Supervisor/DeviationReviewReport"
            },

            new
            {
                Name = "Document Review",
                Description = "Review controlled documents",
                Route = "/Supervisor/DocumentReview"
            }
        };

        return Ok(modules);
    }
}