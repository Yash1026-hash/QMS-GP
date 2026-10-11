using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.DTOs;

namespace QMSSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ChangeRequestsIndexController : ControllerBase
    {
        private readonly UserDbContext _context;

        public ChangeRequestsIndexController(UserDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OperatorChangeRequest>>> GetChangeRequests()
        {
            var changeRequests = await _context.OperatorChangeRequests
                .OrderByDescending(x => x.RequestedDate)
                .ToListAsync();

            return Ok(changeRequests);
        }
    }
}