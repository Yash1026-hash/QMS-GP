using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Enums;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/deviations")]
public class DeviationsController : ControllerBase
{
    private readonly QmsDbContext _context;

    public DeviationsController(QmsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DeviationListResponseDto>> GetDeviations(
        [FromQuery] string? search,
        [FromQuery] Priority? priority,
        [FromQuery] DeviationStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // Validate page number
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        // Validate page size
        if (pageSize < 1)
        {
            pageSize = 10;
        }

        // Prevent very large requests
        if (pageSize > 100)
        {
            pageSize = 100;
        }

        var query = _context.Deviations
            .AsNoTracking()
            .AsQueryable();

        // Search by title, description or operator
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(x =>
                x.Title.Contains(search) ||
                x.Description.Contains(search) ||
                x.CreatedBy.Contains(search));
        }

        // Priority filter
        if (priority.HasValue)
        {
            query = query.Where(x =>
                x.Priority == priority.Value);
        }

        // Status filter
        if (status.HasValue)
        {
            query = query.Where(x =>
                x.Status == status.Value);
        }

        // Total records before pagination
        var totalCount = await query.CountAsync();

        // Get current page
        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DeviationListDto
            {
                Id = x.Id,

                Title = x.Title,

                DocumentId = x.DocumentId,

                Priority = x.Priority,

                Status = x.Status,

                CreatedBy = x.CreatedBy,

                CreatedDate = x.CreatedDate,

                SupervisorComment = x.SupervisorComment
            })
            .ToListAsync();

        var result = new DeviationListResponseDto
        {
            Items = items,

            PageNumber = pageNumber,

            PageSize = pageSize,

            TotalCount = totalCount
        };

        return Ok(result);
    }
}