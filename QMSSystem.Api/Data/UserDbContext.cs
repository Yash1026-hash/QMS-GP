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
    // USERS
    // =========================================================

    public DbSet<UserAccount> Users => Set<UserAccount>();
    public DbSet<DocumentCreation> DocumentCreations => Set<DocumentCreation>();
    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();


    // =========================================================
    // DOCUMENTS
    // =========================================================

    // public DbSet<DocumentCreation> DocumentCreations
    //     => Set<DocumentCreation>();

    public DbSet<DocumentHistory> DocumentHistories
        => Set<DocumentHistory>();


    // =========================================================
    // CHANGE REQUESTS
    // =========================================================

    public DbSet<OperatorChangeRequest> OperatorChangeRequests
        => Set<OperatorChangeRequest>();

    public DbSet<OperatorChangeRequestDeviation>
        OperatorChangeRequestDeviations
        => Set<OperatorChangeRequestDeviation>();


    // =========================================================
    // DEVIATIONS
    // =========================================================

    public DbSet<DeviationRequest> DeviationRequests
        => Set<DeviationRequest>();

    public DbSet<DeviationReportRequest> DeviationReportRequests
        => Set<DeviationReportRequest>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // =====================================================
        // OPERATOR CHANGE REQUEST
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
                .IsRequired();
        
            entity.Property(document => document.FileName)
                .IsRequired();
        
            entity.Property(document => document.ContentType)
                .IsRequired();
        
            entity.Property(document => document.FileData)
                .IsRequired();
        
            entity.Property(document => document.CreatedBy)
                .IsRequired();
        
            entity.Property(document => document.CreationOn)
                .IsRequired();
        
            entity.Property(document => document.Comment)
            .IsRequired(false);    
        });
    
        modelBuilder.Entity<OperatorChangeRequest>(entity =>
        {
            entity.ToTable("OperatorChangeRequests", "dbo");

            entity.HasKey(changeRequest => changeRequest.Id);

            entity.Property(changeRequest => changeRequest.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(changeRequest => changeRequest.ChangeType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(changeRequest => changeRequest.Description)
                .IsRequired();

            entity.Property(changeRequest => changeRequest.Decision)
                .HasMaxLength(20);

            entity.Property(changeRequest => changeRequest.DecisionComment)
                .HasMaxLength(1000);

            entity.Property(changeRequest => changeRequest.Status)
                .HasMaxLength(20)
                .IsRequired();
        });


        // =====================================================
        // OPERATOR CHANGE REQUEST DEVIATION
        // =====================================================

        modelBuilder.Entity<OperatorChangeRequestDeviation>(entity =>
        {
            entity.ToTable(
                "OperatorChangeRequestDeviations",
                "dbo");

            entity.HasKey(deviation => deviation.Id);

            entity.Property(deviation => deviation.ChangeRequestId)
                .IsRequired();

            entity.Property(deviation => deviation.DeviationId)
                .IsRequired();
        });


        // =====================================================
        // USERS
        // =====================================================

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("KS_RecallUsers", "dbo");

            entity.HasKey(user => user.UserId);

            entity.HasIndex(user => user.Username)
                .IsUnique();

            entity.Ignore(user => user.Roles);
        });


        // =====================================================
        // ROLES
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
        // USER ROLES
        // =====================================================

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("KS_UserRoles", "dbo");

            entity.HasKey(userRole =>
                new
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
        // DOCUMENT CREATIONS
        // dbo.DocumentCreations
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
                .IsRequired();

            entity.Property(document => document.FileName)
                .IsRequired();

            entity.Property(document => document.ContentType)
                .IsRequired();

            entity.Property(document => document.FileData)
                .IsRequired();

            entity.Property(document => document.CreatedBy)
                .IsRequired();

            entity.Property(document => document.CreationOn)
                .IsRequired();

            entity.Property(document => document.Comment)
                .IsRequired(false);
        });


        // =====================================================
        // DOCUMENT HISTORY
        // =====================================================

        modelBuilder.Entity<DocumentHistory>(entity =>
        {
            entity.ToTable("DocumentHistory", "dbo");

            entity.HasKey(h => h.Id);

            entity.Property(h => h.DocumentNumber)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(h => h.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(h => h.Department)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(h => h.Status)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(h => h.Comment)
                .HasMaxLength(2000);

            entity.HasIndex(h => h.DocumentId);
        });


        // =====================================================
        // DEVIATION REQUEST
        //
        // Existing model:
        // DeviationRequest.cs
        //
        // Database:
        // dbo.KS_Deviations
        // =====================================================

        modelBuilder.Entity<DeviationRequest>(entity =>
        {
            entity.ToTable("KS_Deviations", "dbo");

            // Primary Key
            entity.HasKey(deviation => deviation.Id);

            // DocumentId
            // FK -> dbo.DocumentCreations.Id
            entity.Property(deviation => deviation.DocumentId)
                .IsRequired();

            // Title
            entity.Property(deviation => deviation.Title)
                .HasMaxLength(250)
                .IsRequired();

            // Description
            entity.Property(deviation => deviation.Description)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            // Priority
            entity.Property(deviation => deviation.Priority)
                .HasMaxLength(50)
                .IsRequired();

            // Status
            // 0 = Pending
            // 1 = Active
            // 2 = Inactive
            entity.Property(deviation => deviation.Status)
                .IsRequired()
                .HasDefaultValue(0);

            // CreatedBy
            entity.Property(deviation => deviation.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();

            // CreatedDate
            entity.Property(deviation => deviation.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            // Decision
            // 1 = Approve
            // 2 = Reject
            entity.Property(deviation => deviation.Decision)
                .IsRequired(false);

            // DecisionBy
            entity.Property(deviation => deviation.DecisionBy)
                .HasMaxLength(150)
                .IsRequired(false);

            // DecisionOn
            entity.Property(deviation => deviation.DecisionOn)
                .IsRequired(false);

            // DecisionComments
            entity.Property(deviation => deviation.DecisionComments)
                .HasColumnType("nvarchar(max)")
                .IsRequired(false);

            // FK:
            // KS_Deviations.DocumentId
            //        ↓
            // DocumentCreations.Id
            entity.HasOne<DocumentCreation>()
                .WithMany()
                .HasForeignKey(deviation => deviation.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // =====================================================
        // DEVIATION REPORT REQUEST
        //
        // Existing model:
        // DeviationReportReq.cs
        //
        // Database:
        // dbo.KS_DeviationReports
        // =====================================================

        modelBuilder.Entity<DeviationReportRequest>(entity =>
        {
            entity.ToTable("KS_DeviationReports", "dbo");

            // Primary Key
            entity.HasKey(report => report.Id);

            // DeviationId
            // FK -> KS_Deviations.Id
            entity.Property(report => report.DeviationId)
                .IsRequired();

            // DocumentId
            // FK -> DocumentCreations.Id
            entity.Property(report => report.DocumentId)
                .IsRequired();

            // Proof
            // VARBINARY(MAX) NULL
            entity.Property(report => report.Proof)
                .HasColumnType("varbinary(max)")
                .IsRequired(false);

            // CreatedBy
            entity.Property(report => report.CreatedBy)
                .HasMaxLength(150)
                .IsRequired();

            // CreatedDate
            entity.Property(report => report.CreatedDate)
                .IsRequired()
                .HasDefaultValueSql("SYSUTCDATETIME()");

            // Summary
            entity.Property(report => report.Summary)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            // Status
            // 0 = Pending
            // 1 = Active
            // 2 = Inactive
            entity.Property(report => report.Status)
                .IsRequired()
                .HasDefaultValue(0);

            // -------------------------------------------------
            // IMPORTANT
            // AttemptNumber was removed from the database.
            // If it exists in the existing model, don't map it.
            // -------------------------------------------------

            entity.Ignore(report => report.AttemptNumber);

            // FK:
            // KS_DeviationReports.DeviationId
            //        ↓
            // KS_Deviations.Id
            entity.HasOne<DeviationRequest>()
                .WithMany()
                .HasForeignKey(report => report.DeviationId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK:
            // KS_DeviationReports.DocumentId
            //        ↓
            // DocumentCreations.Id
            entity.HasOne<DocumentCreation>()
                .WithMany()
                .HasForeignKey(report => report.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
