using AdminService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for KYC reviews and admin activity logs.
/// </summary>
public class AdminDbContext : DbContext
{
    /// <summary>
    /// Initialises the AdminDbContext with the provided EF Core options.
    /// </summary>
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }

    public DbSet<KYCReview> KYCReviews => Set<KYCReview>();
    public DbSet<AdminActivityLog> ActivityLogs => Set<AdminActivityLog>();

    /// <summary>
    /// Configures entity mappings, indexes, and column constraints for the admin schema.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── KYCReview ──
        modelBuilder.Entity<KYCReview>(e =>
        {
            e.HasKey(k => k.Id);
            e.HasIndex(k => k.DocumentId).IsUnique();
            e.HasIndex(k => new { k.UserId, k.Status });
            e.Property(k => k.DocType).HasMaxLength(50).IsRequired();
            e.Property(k => k.FileUrl).HasMaxLength(500).IsRequired();
            e.Property(k => k.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.Property(k => k.ReviewNotes).HasMaxLength(1000);
        });

        // ── AdminActivityLog ──
        modelBuilder.Entity<AdminActivityLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).HasMaxLength(100).IsRequired();
            e.Property(a => a.TargetType).HasMaxLength(50).IsRequired();
            e.Property(a => a.Details).HasMaxLength(2000);
            e.HasIndex(a => new { a.AdminUserId, a.CreatedAt });
        });
    }
}
