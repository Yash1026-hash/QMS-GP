using Microsoft.EntityFrameworkCore;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.DTOs;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Data;

public sealed class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<UserDto> Users => Set<UserDto>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

   public DbSet<DocumentHistory> DocumentHistories => Set<DocumentHistory>();
 

    public DbSet<OperatorChangeRequest> OperatorChangeRequests => Set<OperatorChangeRequest>();

    public DbSet<OperatorChangeRequestDeviation> OperatorChangeRequestDeviations => Set<OperatorChangeRequestDeviation>();

<<<<<<< Updated upstream
 
=======
    

>>>>>>> Stashed changes
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        modelBuilder.Entity<OperatorChangeRequestDeviation>(entity =>
        {
            entity.ToTable("OperatorChangeRequestDeviations", "dbo");

            entity.HasKey(deviation => deviation.Id);

            entity.Property(deviation => deviation.ChangeRequestId)
                .IsRequired();

            entity.Property(deviation => deviation.DeviationId)
                .IsRequired();
        });

        modelBuilder.Entity<UserDto>(entity =>
        {
            entity.ToTable("KS_RecallUsers", "dbo");
            entity.HasKey(user => user.UserId);
            entity.HasIndex(user => user.Username).IsUnique();
            entity.Ignore(user => user.Roles);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("KS_Roles", "dbo");
            entity.HasKey(role => role.RoleId);
            entity.Property(role => role.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(role => role.Name).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("KS_UserRoles", "dbo");
            entity.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
            entity.HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

         modelBuilder.Entity<DocumentHistory>(e =>
{
    e.ToTable("DocumentHistory","dbo");
    e.HasKey(h => h.Id);

    e.Property(h => h.DocumentNumber).HasMaxLength(50).IsRequired();
    e.Property(h => h.Title).HasMaxLength(200).IsRequired();
    e.Property(h => h.Department).HasMaxLength(100).IsRequired();
    e.Property(h => h.Status).HasMaxLength(32).IsRequired();
    e.Property(h => h.Comment).HasMaxLength(2000);

    e.HasIndex(h => h.DocumentId);
});
        
    }
}
