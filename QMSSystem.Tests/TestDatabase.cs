using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Api.Services.Workflow;
using QMSSystem.Shared.Models;

namespace QMSSystem.Tests;

// A fresh in-memory SQLite database per test, with four login users.
public sealed class TestDatabase : IDisposable
{
    public const int Ravi = 1;      // operator
    public const int Priya = 2;     // operator
    public const int Suresh = 3;    // supervisor
    public const int Anita = 4;     // supervisor

    private readonly SqliteConnection qmsConnection = new("DataSource=:memory:");
    private readonly SqliteConnection userConnection = new("DataSource=:memory:");

    public TestDatabase()
    {
        qmsConnection.Open();
        userConnection.Open();

        Context = new QmsDbContext(new DbContextOptionsBuilder<QmsDbContext>().UseSqlite(qmsConnection).Options);
        Context.Database.EnsureCreated();

        Users = new UserDbContext(new DbContextOptionsBuilder<UserDbContext>().UseSqlite(userConnection).Options);
        Users.Database.EnsureCreated();
        Users.Users.AddRange(
            new UserAccount { UserId = Ravi, Username = "ravi", FullName = "Ravi Kumar" },
            new UserAccount { UserId = Priya, Username = "priya", FullName = "Priya Shah" },
            new UserAccount { UserId = Suresh, Username = "suresh", FullName = "Suresh Rao" },
            new UserAccount { UserId = Anita, Username = "anita", FullName = "Anita Das" });
        Users.SaveChanges();

        Workflow = new WorkflowService(Context, new AuditService(Context, Clock), Clock);
        Queries = new WorkflowQueries(Context, Users);
        Modules = new ModuleActions(Context, Workflow, Clock);
    }

    public TestClock Clock { get; } = new(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero));

    public QmsDbContext Context { get; }

    public UserDbContext Users { get; }

    public WorkflowService Workflow { get; }

    public WorkflowQueries Queries { get; }

    public ModuleActions Modules { get; }

    public void Dispose()
    {
        Context.Dispose();
        Users.Dispose();
        qmsConnection.Dispose();
        userConnection.Dispose();
    }
}

// A clock the tests can move forward, so timeline steps get distinct times.
public sealed class TestClock(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset now = start;

    public DateTime Now => now.UtcDateTime;

    public override DateTimeOffset GetUtcNow() => now;

    public void Advance(TimeSpan by) => now = now.Add(by);
}
