using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/supervisor")]
[Authorize(Policy = "SupervisorOnly")]
public class SupervisorController : ControllerBase
{
    private readonly UserDbContext _context;

    public SupervisorController(UserDbContext context)
    {
        _context = context;
    }

    [HttpGet("change-requests/count")]
    public async Task<IActionResult> GetPendingChangeRequestCount()
    {
        int count = await _context.OperatorChangeRequests
            .CountAsync(x => x.Status == "Pending");

        return Ok(count);
    }

    [HttpGet("deviations/count")]
    public async Task<IActionResult> GetPendingDeviationCount()
    {
        int count = await _context.DeviationRequests
            .CountAsync(x => x.Status == 0);

        return Ok(count);
    }
}