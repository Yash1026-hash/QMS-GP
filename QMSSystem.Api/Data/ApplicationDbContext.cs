using Microsoft.EntityFrameworkCore;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    //for dbo
    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Document>(entity =>
    {
        entity.ToTable("Documents", "dbo");
    });
        modelBuilder.Entity<DocumentRevision>(entity =>
        {
            entity.ToTable("DocumentRevisions","dbo");
        });
}
    public DbSet<Document> Documents { get; set; }
    public DbSet<DocumentRevision> DocumentRevisions {get; set;}
}