using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Services;

public class DeviationService
{
    private readonly UserDbContext _context;

    public DeviationService(UserDbContext context)
    {
        _context = context;
    }

    // Get all deviations
    public async Task<List<DeviationRequestDto>> GetDeviationsAsync()
    {
        return await _context.DeviationRequests
            .AsNoTracking()
            .Select(d => new DeviationRequestDto
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                DocumentId = d.DocumentId,
                Priority = d.Priority,
                Status = d.Status,
                CreatedBy = d.CreatedBy,
                CreatedDate = d.CreatedDate,
                DecisionBy = d.DecisionBy,
                DecisionOn = d.DecisionOn,
                Decision = d.Decision,
                DecisionComments = d.DecisionComments
            })
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    // Get one deviation
    public async Task<DeviationRequestDto?> GetDeviationAsync(int id)
    {
        return await _context.DeviationRequests
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new DeviationRequestDto
            {
                Id = d.Id,
                Title = d.Title,
                Description = d.Description,
                DocumentId = d.DocumentId,
                Priority = d.Priority,
                Status = d.Status,
                CreatedBy = d.CreatedBy,
                CreatedDate = d.CreatedDate,
                DecisionBy = d.DecisionBy,
                DecisionOn = d.DecisionOn,
                Decision = d.Decision,
                DecisionComments = d.DecisionComments
            })
            .FirstOrDefaultAsync();
    }

    // Create deviation
    public async Task<DeviationRequestDto> CreateDeviationAsync(
        DeviationRequestDto dto)
    {
        var deviation = new DeviationRequest
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            DocumentId = dto.DocumentId,
            Priority = dto.Priority.Trim(),

            // 0 = Pending
            Status = 0,

            CreatedBy = dto.CreatedBy,
            CreatedDate = DateTime.UtcNow,

            // Supervisor decision is not created here.
            Decision = null,
            DecisionBy = string.Empty,
            DecisionOn = null,
            DecisionComments = string.Empty
        };

        _context.DeviationRequests.Add(deviation);

        await _context.SaveChangesAsync();

        return new DeviationRequestDto
        {
            Id = deviation.Id,
            Title = deviation.Title,
            Description = deviation.Description,
            DocumentId = deviation.DocumentId,
            Priority = deviation.Priority,
            Status = deviation.Status,
            CreatedBy = deviation.CreatedBy,
            CreatedDate = deviation.CreatedDate,
            DecisionBy = deviation.DecisionBy,
            DecisionOn = deviation.DecisionOn,
            Decision = deviation.Decision,
            DecisionComments = deviation.DecisionComments
        };
    }
}