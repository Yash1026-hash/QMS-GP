using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.DTOs;

namespace QMSSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OperatorChangeRequestsController : ControllerBase
    {
        private readonly UserDbContext _context;

        public OperatorChangeRequestsController(UserDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OperatorChangeRequest>> GetById(int id)
        {
            var changeRequest = await _context.OperatorChangeRequests
                .FirstOrDefaultAsync(x => x.Id == id);

            if (changeRequest == null)
            {
                return NotFound();
            }

            return Ok(changeRequest);
        }
    }
}