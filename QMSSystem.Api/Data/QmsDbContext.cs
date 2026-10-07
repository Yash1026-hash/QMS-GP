using Microsoft.EntityFrameworkCore;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Data;

// One DbContext for all QMS module tables (documents, deviations, change requests).
// Every module service uses this context, so a module change and the workflow
// change it triggers are saved together in one SaveChanges call.
// Login tables stay in UserDbContext.
public sealed class QmsDbContext(DbContextOptions<QmsDbContext> options) : DbContext(options)
{
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentRevision> DocumentRevisions => Set<DocumentRevision>();
    public DbSet<Deviation> Deviations => Set<Deviation>();
    public DbSet<DeviationAttachment> DeviationAttachments => Set<DeviationAttachment>();
    public DbSet<DeviationReport> DeviationReports => Set<DeviationReport>();
    public DbSet<ChangeRequest> ChangeRequests { get; set; }
    public DbSet<OperatorChangeRequestDeviation> OperatorChangeRequestDeviations { get; set; }
    public DbSet<ApprovalRecord> ApprovalRecords => Set<ApprovalRecord>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("KS_Documents", "dbo");
            entity.Property(document => document.DocumentNumber).HasMaxLength(50).IsRequired();
            entity.HasIndex(document => document.DocumentNumber).IsUnique();
            entity.Property(document => document.Title).HasMaxLength(200).IsRequired();
            entity.Property(document => document.Department).HasMaxLength(100);
            entity.Property(document => document.Status).HasMaxLength(20);
        });

        modelBuilder.Entity<DocumentRevision>(entity =>
        {
            entity.ToTable("KS_DocumentRevisions", "dbo");
            entity.Property(revision => revision.FileName).HasMaxLength(260);
            entity.Property(revision => revision.ApprovalStatus).HasMaxLength(20);
            entity.HasOne(revision => revision.Document)
                .WithMany()
                .HasForeignKey(revision => revision.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
            
        });

        modelBuilder.Entity<Deviation>(entity =>
        {
            entity.ToTable("KS_Deviations", "dbo");
            entity.Property(deviation => deviation.Title).HasMaxLength(200).IsRequired();
            entity.Property(deviation => deviation.Priority).HasConversion<string>().HasMaxLength(20);
            entity.Property(deviation => deviation.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasOne<Document>()
                .WithMany()
                .HasForeignKey(deviation => deviation.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(deviation => deviation.Attachments)
                .WithOne()
                .HasForeignKey(attachment => attachment.DeviationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(deviation => deviation.Reports)
                .WithOne(report => report.Deviation)
                .HasForeignKey(report => report.DeviationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DeviationAttachment>(entity =>
        {
            entity.ToTable("KS_DeviationAttachments", "dbo");
            entity.Property(attachment => attachment.FileName).HasMaxLength(260);
        });

        modelBuilder.Entity<DeviationReport>(entity =>
        {
            entity.ToTable("KS_DeviationReports", "dbo");
            entity.Property(report => report.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(report => report.FileName).HasMaxLength(260);
            entity.HasIndex(report => new { report.DeviationId, report.AttemptNumber }).IsUnique();
        });

        modelBuilder.Entity<ChangeRequest>(entity =>
        {
            entity.ToTable("OperatorChangeRequests", "dbo");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.ChangeType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Description)
                .IsRequired();

            entity.Property(x => x.RequestedDate)
                .IsRequired();
        });

        modelBuilder.Entity<OperatorChangeRequestDeviation>(entity =>
        {
            entity.ToTable("OperatorChangeRequestDeviations", "dbo");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ChangeRequestId)
                .IsRequired();

            entity.Property(x => x.DeviationId)
                .IsRequired();
        });


        modelBuilder.Entity<ApprovalRecord>(entity =>
        {
            entity.ToTable("KS_ApprovalRecords", "dbo");
            entity.Property(record => record.ItemType).HasMaxLength(30).IsRequired();
            entity.Property(record => record.Decision).HasMaxLength(20).IsRequired();
            // One decision per item (change request rule 4). Each report attempt is its own item.
            entity.HasIndex(record => new { record.ItemType, record.ItemId }).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("KS_AuditLogs", "dbo");
            entity.Property(log => log.ItemType).HasMaxLength(30).IsRequired();
            entity.Property(log => log.Action).HasMaxLength(100).IsRequired();
            entity.HasIndex(log => log.DeviationId);
            entity.HasIndex(log => new { log.ItemType, log.ItemId });
        });

       

    }
}
