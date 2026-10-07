using Microsoft.EntityFrameworkCore;
using QMSSystem.Shared.Models;
using static QMSSystem.Tests.TestDatabase;

namespace QMSSystem.Tests;

// The supervisor's review comments and proof files on a deviation report.
public sealed class ReviewRecordTests : IDisposable
{
    private readonly TestDatabase db = new();

    public void Dispose() => db.Dispose();

    [Fact]
    public async Task Review_comment_and_proof_are_saved_against_the_report()
    {
        var version = await db.Modules.CreateDocumentAsync("SOP-020", Ravi);
        await db.Modules.ApproveRevisionAsync(version.Id, Suresh);
        var deviation = await db.Modules.RaiseDeviationAsync(version.DocumentId, Ravi);
        await db.Modules.ReviewDeviationAsync(deviation.Id, Suresh, accept: true);
        var report = await db.Modules.SubmitReportAsync(deviation.Id, Ravi);

        db.Context.ReviewComments.Add(new ReviewComment
        {
            DeviationReportId = report.Id,
            Comment = "Root cause is clear.",
            CommentedBy = Suresh.ToString(),
            CommentDate = db.Clock.Now
        });
        db.Context.ReviewProofs.Add(new ReviewProof
        {
            DeviationReportId = report.Id,
            ProofName = "line-check.jpg",
            ProofType = "Image",
            FilePath = "proofs/line-check.jpg",
            UploadedDate = db.Clock.Now
        });
        await db.Context.SaveChangesAsync();

        Assert.Equal(Suresh.ToString(),
            (await db.Context.ReviewComments.AsNoTracking().SingleAsync(item => item.DeviationReportId == report.Id)).CommentedBy);
        Assert.Equal("line-check.jpg",
            (await db.Context.ReviewProofs.AsNoTracking().SingleAsync(item => item.DeviationReportId == report.Id)).ProofName);
    }

    [Fact]
    public async Task Review_comment_needs_an_existing_report()
    {
        db.Context.ReviewComments.Add(new ReviewComment { DeviationReportId = 999, CommentedBy = Suresh.ToString() });

        await Assert.ThrowsAsync<DbUpdateException>(() => db.Context.SaveChangesAsync());
    }
}
