using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Services;

public class DeviationReviewService
{
    private readonly UserDbContext _context;

    public DeviationReviewService(UserDbContext context)
    {
        _context = context;
    }

    public async Task<DeviationRequestDto?> GetDeviationByIdAsync(int id)
    {
        var deviation = await _context.DeviationRequests
            .FirstOrDefaultAsync(x => x.Id == id);

        if (deviation == null)
        {
            return null;
        }

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
            Decision = deviation.Decision,
            DecisionBy = deviation.DecisionBy,
            DecisionOn = deviation.DecisionOn,
            DecisionComments = deviation.DecisionComments
        };
    }

    public async Task<bool> SubmitDecisionAsync(
        int deviationId,
        int decision,
        string decisionComments,
        string decisionBy)
    {
        var deviation = await _context.DeviationRequests
            .FirstOrDefaultAsync(x => x.Id == deviationId);

        if (deviation == null)
        {
            return false;
        }

        if (deviation.Status != 0)
        {
            return false;
        }

        if (decision != 1 && decision != 2)
        {
            return false;
        }

        if (decision == 2 && string.IsNullOrWhiteSpace(decisionComments))
        {
            return false;
        }

        deviation.Decision = decision;
        deviation.DecisionBy = decisionBy;
        deviation.DecisionOn = DateTime.UtcNow;
        deviation.DecisionComments = decisionComments?.Trim() ?? string.Empty;

        if (decision == 1)
        {
            deviation.Status = 1;
        }
        else
        {
            deviation.Status = 2;
        }

        await _context.SaveChangesAsync();

        return true;
    }
}