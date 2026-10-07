using Microsoft.EntityFrameworkCore;
using QMSSystem.Shared.Models;

namespace QMSSystem.API.Data;

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
}
    public DbSet<Document> Documents { get; set; }
}