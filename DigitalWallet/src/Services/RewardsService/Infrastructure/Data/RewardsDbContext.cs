using Microsoft.EntityFrameworkCore;
using RewardsService.Domain.Entities;

namespace RewardsService.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the RewardsService, covering accounts, transactions, rules, catalog, and redemptions.
/// </summary>
public class RewardsDbContext : DbContext
{
    /// <summary>
    /// Initializes the context with the provided EF Core options.
    /// </summary>
    public RewardsDbContext(DbContextOptions<RewardsDbContext> options) : base(options) { }

    /// <summary>
    /// Table of rewards accounts, one per application user.
    /// </summary>
    public DbSet<RewardsAccount> RewardsAccounts => Set<RewardsAccount>();
    /// <summary>
    /// Table of all points transactions (earn, redeem, expire, adjust).
    /// </summary>
    public DbSet<RewardsTransaction> RewardsTransactions => Set<RewardsTransaction>();
    /// <summary>
    /// Table of earn rules that define points awarded per trigger type.
    /// </summary>
    public DbSet<EarnRule> EarnRules => Set<EarnRule>();
    /// <summary>
    /// Table of catalog items available for redemption.
    /// </summary>
    public DbSet<RewardsCatalogItem> CatalogItems => Set<RewardsCatalogItem>();
    /// <summary>
    /// Table of user redemption records.
    /// </summary>
    public DbSet<Redemption> Redemptions => Set<Redemption>();

    /// <summary>
    /// Configures entity mappings, indexes, constraints, and seeds default earn rules.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── RewardsAccount ──
        modelBuilder.Entity<RewardsAccount>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.UserId).IsUnique();
            e.Property(r => r.Tier).HasMaxLength(20).HasDefaultValue("Bronze");
        });

        // ── RewardsTransaction ──
        modelBuilder.Entity<RewardsTransaction>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Type).HasMaxLength(20).IsRequired();
            e.Property(t => t.ReferenceType).HasMaxLength(50);
            e.Property(t => t.Description).HasMaxLength(500);
            e.HasIndex(t => new { t.RewardsAccountId, t.CreatedAt });
            e.HasOne(t => t.RewardsAccount)
             .WithMany(r => r.Transactions)
             .HasForeignKey(t => t.RewardsAccountId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── EarnRule ──
        modelBuilder.Entity<EarnRule>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.TriggerType).HasMaxLength(50);
            e.Property(r => r.PointsPerRupee).HasColumnType("decimal(10,4)");
        });

        // ── RewardsCatalogItem ──
        modelBuilder.Entity<RewardsCatalogItem>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(200).IsRequired();
            e.Property(c => c.Category).HasMaxLength(50);
        });

        // ── Redemption ──
        modelBuilder.Entity<Redemption>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.Property(r => r.FulfillmentCode).HasMaxLength(256);
            e.HasOne(r => r.RewardsAccount)
             .WithMany(ra => ra.Redemptions)
             .HasForeignKey(r => r.RewardsAccountId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.CatalogItem)
             .WithMany(c => c.Redemptions)
             .HasForeignKey(r => r.CatalogItemId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Seed default earn rules ──
        modelBuilder.Entity<EarnRule>().HasData(
            new EarnRule
            {
                Id = Guid.Parse("11111111-0000-0000-0000-000000000001"),
                Name = "TopUp Earn",
                TriggerType = "TopUp",
                PointsPerRupee = 0.01m,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new EarnRule
            {
                Id = Guid.Parse("11111111-0000-0000-0000-000000000002"),
                Name = "Transfer Earn",
                TriggerType = "Transfer",
                PointsPerRupee = 0.005m,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
