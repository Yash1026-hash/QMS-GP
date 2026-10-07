using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Enums;
using static QMSSystem.Tests.TestDatabase;

namespace QMSSystem.Tests;

// The demo data must put one record at every workflow step, and every
// picker must find exactly the record that is waiting at its step.
public sealed class DemoDataSeederTests : IDisposable
{
    private readonly TestDatabase db = new();

    public void Dispose() => db.Dispose();

    private Task<bool> SeedAsync() =>
        DemoDataSeeder.SeedAsync(db.Context, new DemoUserIds(Ravi, Priya, Suresh, Anita), db.Clock.Now);

    [Fact]
    public async Task Demo_data_has_one_record_at_every_step()
    {
        Assert.True(await SeedAsync());

        var statuses = await db.Context.Deviations.Select(deviation => deviation.Status).ToListAsync();
        foreach (var status in Enum.GetValues<DeviationStatus>())
        {
            Assert.Contains(status, statuses);
        }

        Assert.Equal(["SOP-001", "SOP-002"], (await db.Queries.GetActiveDocumentsAsync()).Select(document => document.DocumentNumber));
        Assert.Equal(["Sample tested at 40 C instead of 37 C"], (await db.Queries.GetDeviationsReadyForReportAsync()).Select(item => item.Title));
        Assert.Equal(["Thermometer calibration expired"], (await db.Queries.GetReportsReadyForChangeRequestAsync()).Select(item => item.DeviationTitle));

        var sop1 = await db.Context.Documents.SingleAsync(document => document.DocumentNumber == "SOP-001");
        Assert.Equal(["Add a temperature log sheet step"], (await db.Queries.GetChangeRequestsReadyForRevisionAsync(sop1.Id)).Select(item => item.Title));
    }

    [Fact]
    public async Task Demo_full_chain_deviation_has_a_complete_timeline()
    {
        await SeedAsync();
        var deviation = await db.Context.Deviations.SingleAsync(item => item.Title == "Mixer speed above limit");

        var timeline = await db.Queries.GetTimelineAsync(deviation.Id);

        Assert.Equal("Deviation raised", timeline![0].Step);
        Assert.Contains(timeline, entry => entry.Step == "Revision 2 approved");
        Assert.Equal("Deviation closed", timeline[^1].Step);
        Assert.Equal("Anita Das", timeline[^1].Who);
    }

    [Fact]
    public async Task Demo_data_is_not_added_twice()
    {
        Assert.True(await SeedAsync());
        Assert.False(await SeedAsync());
    }
}
