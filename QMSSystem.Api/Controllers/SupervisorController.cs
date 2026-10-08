using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/supervisor")]
[Authorize(Policy = "SupervisorOnly")]
public class SupervisorController : ControllerBase
{
    [HttpGet("operations")]
    public IActionResult GetOperations()
    {
        var operations = new
        {
            TotalPending = 14,

            DeviationReview = new
            {
                Name = "Deviation Review",
                Count = 3,
                Route = "/Supervisor/DeviationReview"
            },

            ChangeRequestApproval = new
            {
                Name = "Change Request Approval",
                Count = 4,
                Route = "/Supervisor/ChangeRequestApproval"
            },

            DeviationReviewReport = new
            {
                Name = "Deviation Review Report",
                Count = 2,
                Route = "/Supervisor/DeviationReviewReport"
            },

            DocumentReview = new
            {
                Name = "Document Review",
                Count = 5,
                Route = "/Supervisor/DocumentReview"
            }
        };

        return Ok(operations);
    }
}