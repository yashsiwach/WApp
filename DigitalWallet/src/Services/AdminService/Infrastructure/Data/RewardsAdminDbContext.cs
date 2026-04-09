using AdminService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Data;

public class RewardsAdminDbContext : DbContext
{
    public RewardsAdminDbContext(DbContextOptions<RewardsAdminDbContext> options) : base(options) { }

    public DbSet<RewardsCatalogItem> CatalogItems => Set<RewardsCatalogItem>();

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
