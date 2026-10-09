using Microsoft.EntityFrameworkCore;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.DTOs;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Data;

public sealed class UserDbContext(DbContextOptions<UserDbContext> options)
    : DbContext(options)
{
    // =========================================================
    // USERS & ROLES
    // =========================================================

    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<DocumentCreation> DocumentCreations => Set<DocumentCreation>();
    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // =========================================================
    // DOCUMENTS
    // =========================================================

    // public DbSet<DocumentCreation> DocumentCreations => Set<DocumentCreation>();

    public DbSet<DocumentHistory> DocumentHistories => Set<DocumentHistory>();


    // =========================================================
    // CHANGE REQUESTS
    // =========================================================

    public DbSet<OperatorChangeRequest> OperatorChangeRequests => Set<OperatorChangeRequest>();

    public DbSet<OperatorChangeRequestDeviation> OperatorChangeRequestDeviations => Set<OperatorChangeRequestDeviation>();


    // =========================================================
    // DEVIATIONS
    // =========================================================

    public DbSet<DeviationRequest> DeviationRequests => Set<DeviationRequest>();

    public DbSet<DeviationReportRequest> DeviationReportRequests => Set<DeviationReportRequest>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // =====================================================
        // USERS (dbo.KS_RecallUsers)
        // =====================================================

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("KS_RecallUsers", "dbo");

            entity.HasKey(user => user.UserId);

            entity.Property(user => user.Username)
                .HasMaxLength(450)
                .IsRequired();

            entity.HasIndex(user => user.Username)
                .IsUnique();

            entity.Property(user => user.Password)
                .IsRequired();

            entity.Property(user => user.Role)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(user => user.FullName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(user => user.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(user => user.RegistrationStatus)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(user => user.Department)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(user => user.IsActive)
                .IsRequired();

            entity.Property(user => user.CreatedAt)
                .IsRequired();

            entity.Ignore(user => user.Roles);
        });


        // =====================================================
        // ROLES (dbo.KS_Roles)
        // =====================================================

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("KS_Roles", "dbo");

            entity.HasKey(role => role.RoleId);

            entity.Property(role => role.Name)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(role => role.Name)
                .IsUnique();
        });


        // =====================================================
        // USER ROLES (dbo.KS_UserRoles)
        // =====================================================

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("KS_UserRoles", "dbo");

            entity.HasKey(userRole => new
            {
                userRole.UserId,
                userRole.RoleId
            });

            entity.HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // =====================================================
        // DOCUMENT CREATIONS (dbo.DocumentCreations)
        // =====================================================

        modelBuilder.Entity<DocumentCreation>(entity =>
        {
            entity.ToTable("DocumentCreations", "dbo");

            entity.HasKey(document => document.Id);

            entity.Property(document => document.DocumentNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(document => document.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(document => document.Department)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(document => document.DocumentVersion)
                .IsRequired();

            entity.Property(document => document.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(document => document.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(document => document.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(document => document.FileData)
                .IsRequired();

            entity.Property(document => document.CreatedBy)
                .IsRequired();

            entity.Property(document => document.CreationOn)
                .IsRequired();

            entity.Property(document => document.Comment)
                .IsRequired(false);

            entity.Property(document => document.DecisionBy)
                .HasColumnName("decisionBy")
                .HasColumnType("varchar")
                .IsRequired(false);

            entity.Property(document => document.DecisionDate)
                .HasColumnName("decisionDate")
                .HasColumnType("datetime")
                .IsRequired(false);

            entity.Property(document => document.DecisionStatus)
                .HasColumnName("decision")
                .IsRequired(false);
        });


        // =====================================================
        // DOCUMENT HISTORY (dbo.DocumentHistory)
        // =====================================================

        modelBuilder.Entity<DocumentHistory>(entity =>
        {
            entity.ToTable("DocumentHistory", "dbo");

            entity.HasKey(h => h.Id);

            entity.Property(h => h.DocumentId)
                .IsRequired();

            entity.Property(h => h.DocumentNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(h => h.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(h => h.Department)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(h => h.DocumentVersion)
                .IsRequired();

            entity.Property(h => h.Status)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(h => h.FileName)
                .IsRequired();

            entity.Property(h => h.ContentType)
                .IsRequired();

            entity.Property(h => h.FileData)
                .IsRequired();

            entity.Property(h => h.CreatedBy)
                .IsRequired();

            entity.Property(h => h.CreationOn)
                .IsRequired();

            entity.Property(h => h.Comment)
                .HasMaxLength(2000)
                .IsRequired(false);

            entity.Property(h => h.ArchivedOn)
                .IsRequired();

            entity.Property(h => h.ArchivedBy)
                .IsRequired();

            entity.HasIndex(h => h.DocumentId);
        });


        // =====================================================
        // OPERATOR CHANGE REQUESTS (dbo.OperatorChangeRequests)
        // =====================================================

        modelBuilder.Entity<OperatorChangeRequest>(entity =>
        {
            entity.ToTable("OperatorChangeRequests", "dbo");

            entity.HasKey(changeRequest => changeRequest.Id);

            entity.Property(changeRequest => changeRequest.DocumentId)
                .IsRequired();

            entity.Property(changeRequest => changeRequest.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(changeRequest => changeRequest.ChangeType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(changeRequest => changeRequest.Description)
                .IsRequired();

            entity.Property(changeRequest => changeRequest.RequestedByUserId)
                .IsRequired();

            entity.Property(changeRequest => changeRequest.RequestedDate)
                .IsRequired();

            entity.Property(changeRequest => changeRequest.Decision)
                .HasMaxLength(20)
                .IsRequired(false);

            entity.Property(changeRequest => changeRequest.DecisionComment)
                .HasMaxLength(1000)
                .IsRequired(false);

            entity.Property(changeRequest => changeRequest.DecisionDate)
                .IsRequired(false);

            entity.Property(changeRequest => changeRequest.Status)
                .HasMaxLength(20)
                .IsRequired();
        });


        // =====================================================
        // OPERATOR CHANGE REQUEST DEVIATIONS
        // dbo.OperatorChangeRequestDeviations
        // =====================================================

        modelBuilder.Entity<OperatorChangeRequestDeviation>(entity =>
        {
            entity.ToTable("OperatorChangeRequestDeviations", "dbo");

            entity.HasKey(deviation => deviation.Id);

            entity.Property(deviation => deviation.ChangeRequestId)
                .IsRequired();

            entity.Property(deviation => deviation.DeviationId)
                .IsRequired();
        });


        // =====================================================
        // DEVIATION REQUESTS (dbo.KS_Deviations)
        // =====================================================

        modelBuilder.Entity<DeviationRequest>(entity =>
        {
            entity.ToTable("KS_Deviations", "dbo");

            entity.HasKey(deviation => deviation.Id);

            entity.Property(deviation => deviation.DocumentId)
                .IsRequired();

            entity.Property(deviation => deviation.Title)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(deviation => deviation.Description)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            entity.Property(deviation => deviation.Priority)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(deviation => deviation.Status)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(deviation => deviation.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(deviation => deviation.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(deviation => deviation.Decision)
                .IsRequired(false);

            entity.Property(deviation => deviation.DecisionBy)
                .HasMaxLength(150)
                .IsRequired(false);

            entity.Property(deviation => deviation.DecisionOn)
                .IsRequired(false);

            entity.Property(deviation => deviation.DecisionComments)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            entity.HasOne<DocumentCreation>()
                .WithMany()
                .HasForeignKey(deviation => deviation.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // =====================================================
        // DEVIATION REPORT REQUESTS (dbo.KS_DeviationReports)
        // =====================================================

        modelBuilder.Entity<DeviationReportRequest>(entity =>
        {
            entity.ToTable("KS_DeviationReports", "dbo");

            entity.HasKey(report => report.Id);

            entity.Property(report => report.DeviationId)
                .IsRequired();

            entity.Property(report => report.DocumentId)
                .IsRequired();

            entity.Property(report => report.Proof)
                .HasColumnType("varbinary(max)")
                .IsRequired(false);

            entity.Property(report => report.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(report => report.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.Property(report => report.Summary)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            entity.Property(report => report.Status)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(report => report.Decision)
                .IsRequired(false);

            entity.Property(report => report.DecisionBy)
                .HasMaxLength(150)
                .IsRequired(false);

            entity.Property(report => report.DecisionOn)
                .IsRequired(false);

            entity.Property(report => report.DecisionComments)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            entity.Ignore(report => report.AttemptNumber);

            entity.HasOne<DeviationRequest>()
                .WithMany()
                .HasForeignKey(report => report.DeviationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<DocumentCreation>()
                .WithMany()
                .HasForeignKey(report => report.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}