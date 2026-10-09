using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;

namespace QMSSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChangeRequestsIndexController : ControllerBase
    {
        private readonly UserDbContext _context;

        public ChangeRequestsIndexController(UserDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetChangeRequests()
        {
            var changeRequests = await _context.OperatorChangeRequests
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.ChangeType,
                    x.RequestedDate
                })
                .ToListAsync();

            return Ok(changeRequests);
        }
    }
}