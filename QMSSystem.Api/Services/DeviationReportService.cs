using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Services;

public class DeviationReportService
{
    private readonly UserDbContext _context;

    public DeviationReportService(UserDbContext context)
    {
        _context = context;
    }

    // Get all reports for a deviation
    public async Task<List<DeviationReportDto>?> GetReportsAsync(
        int deviationId)
    {
        var deviationExists = await _context.DeviationRequests
            .AnyAsync(d => d.Id == deviationId);

        if (!deviationExists)
        {
            return null;
        }

        return await _context.DeviationReportRequests
            .AsNoTracking()
            .Where(r => r.DeviationId == deviationId)
            .Select(r => new DeviationReportDto
            {
                Id = r.Id,
                DeviationId = r.DeviationId,
                DocumentId = r.DocumentId,
                AttemptNumber = r.AttemptNumber,
                Summary = r.Summary,
                Proof = r.Proof,
                CreatedBy = r.CreatedBy,
                CreatedDate = r.CreatedDate,
                Status = r.Status,

                // Approval information will be handled
                // by the supervisor workflow later.
                DecisionComments = string.Empty,
                DecisionBy = string.Empty,
                DecisionOn = null,
                Decision = null
            })
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    // Get one report
    public async Task<DeviationReportDto?> GetReportAsync(int id)
    {
        return await _context.DeviationReportRequests
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new DeviationReportDto
            {
                Id = r.Id,
                DeviationId = r.DeviationId,
                DocumentId = r.DocumentId,
                AttemptNumber = r.AttemptNumber,
                Summary = r.Summary,
                Proof = r.Proof,
                CreatedBy = r.CreatedBy,
                CreatedDate = r.CreatedDate,
                Status = r.Status,

                DecisionComments = string.Empty,
                DecisionBy = string.Empty,
                DecisionOn = null,
                Decision = null
            })
            .FirstOrDefaultAsync();
    }

    // Create report
    public async Task<DeviationReportDto> CreateReportAsync(
        DeviationReportDto dto)
    {
        var report = new DeviationReportRequest
        {
            DeviationId = dto.DeviationId,
            DocumentId = dto.DocumentId,
            Summary = dto.Summary.Trim(),
            Proof = dto.Proof,
            CreatedBy = dto.CreatedBy,
            CreatedDate = DateTime.UtcNow,

            // 0 = Pending
            Status = 0
        };

        _context.DeviationReportRequests.Add(report);

        await _context.SaveChangesAsync();

        return new DeviationReportDto
        {
            Id = report.Id,
            DeviationId = report.DeviationId,
            DocumentId = report.DocumentId,
            AttemptNumber = dto.AttemptNumber,
            Summary = report.Summary,
            Proof = report.Proof,
            CreatedBy = report.CreatedBy,
            CreatedDate = report.CreatedDate,
            Status = report.Status,

            DecisionComments = string.Empty,
            DecisionBy = string.Empty,
            DecisionOn = null,
            Decision = null
        };
    }
}