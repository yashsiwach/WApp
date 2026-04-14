using AdminService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the rewards catalog managed by admins.
/// </summary>
public class RewardsAdminDbContext : DbContext
{
    /// <summary>
    /// Initialises the RewardsAdminDbContext with the provided EF Core options.
    /// </summary>
    public RewardsAdminDbContext(DbContextOptions<RewardsAdminDbContext> options) : base(options) { }

    public DbSet<RewardsCatalogItem> CatalogItems => Set<RewardsCatalogItem>();

    /// <summary>
    /// Configures entity mappings, table names, and column constraints for the rewards catalog schema.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RewardsCatalogItem>(e =>
        {
            e.ToTable("CatalogItems");
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Description).HasMaxLength(1000).IsRequired();
            e.Property(c => c.Category).HasMaxLength(50).IsRequired();
        });
    }
}
