
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/operator")]
public class OperatorDashboardController(UserDbContext db)
    : ControllerBase
{
    [HttpGet("counts")]
    public async Task<IActionResult> GetCounts()
    {
        var counts = new
        {
            Documents = await db.DocumentCreations.CountAsync(),

            Deviations = await db.DeviationRequests.CountAsync(),

            DeviationReports = await db.DeviationReportRequests.CountAsync(),

            ChangeRequests = await db.OperatorChangeRequests.CountAsync()
        };

        return Ok(counts);
    }
}
