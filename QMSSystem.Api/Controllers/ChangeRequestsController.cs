using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.DTOs;

namespace QMSSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChangeRequestController : ControllerBase
    {
        private readonly UserDbContext _context;

        public ChangeRequestController(UserDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<OperatorChangeRequest>>> GetAll()
        {
            var changeRequests = await _context.OperatorChangeRequests
                .ToListAsync();

            return Ok(changeRequests);
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