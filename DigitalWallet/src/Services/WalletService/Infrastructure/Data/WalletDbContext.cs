using Microsoft.EntityFrameworkCore;
using WalletService.Domain.Entities;

namespace WalletService.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the WalletService, exposing all wallet-related entity sets.
/// </summary>
public class WalletDbContext : DbContext
{
    /// <summary>
    /// Initializes the database context with the provided EF Core options.
    /// </summary>
    public WalletDbContext(DbContextOptions<WalletDbContext> options) : base(options) { }

    public DbSet<WalletAccount> WalletAccounts => Set<WalletAccount>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<TopUpRequest> TopUpRequests => Set<TopUpRequest>();
    public DbSet<TransferRequest> TransferRequests => Set<TransferRequest>();
    public DbSet<DailyLimitTracker> DailyLimitTrackers => Set<DailyLimitTracker>();

    /// <summary>
    /// Configures entity schemas, constraints, indexes, and relationships for all wallet domain types.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // —— WalletAccount ——
        modelBuilder.Entity<WalletAccount>(e =>
        {
            e.HasKey(w => w.Id);
            e.HasIndex(w => w.UserId).IsUnique();
            e.HasIndex(w => w.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
            e.Property(w => w.Email).HasMaxLength(256);
            e.Property(w => w.SnapshotBalance).HasColumnType("decimal(18,2)");
            e.Property(w => w.Currency).HasMaxLength(10).HasDefaultValue("INR");
        });

        // —— LedgerEntry ——
        modelBuilder.Entity<LedgerEntry>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Amount).HasColumnType("decimal(18,2)");
            e.Property(l => l.Type).HasMaxLength(10).IsRequired();
            e.Property(l => l.ReferenceType).HasMaxLength(50);
            e.Property(l => l.Description).HasMaxLength(500);
            e.HasIndex(l => new { l.WalletId, l.CreatedAt });
            e.HasOne(l => l.Wallet)
             .WithMany(w => w.LedgerEntries)
             .HasForeignKey(l => l.WalletId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // —— TopUpRequest ——
        modelBuilder.Entity<TopUpRequest>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            e.Property(t => t.Provider).HasMaxLength(100);
            e.HasIndex(t => t.IdempotencyKey).IsUnique();
            e.Property(t => t.IdempotencyKey).HasMaxLength(256).IsRequired();
            e.Property(t => t.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.HasOne(t => t.Wallet)
             .WithMany(w => w.TopUpRequests)
             .HasForeignKey(t => t.WalletId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // —— TransferRequest ——
        modelBuilder.Entity<TransferRequest>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            e.HasIndex(t => t.IdempotencyKey).IsUnique();
            e.Property(t => t.IdempotencyKey).HasMaxLength(256).IsRequired();
            e.Property(t => t.Status).HasMaxLength(20).HasDefaultValue("Pending");
        });

        // —— DailyLimitTracker ——
        modelBuilder.Entity<DailyLimitTracker>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasIndex(d => new { d.WalletId, d.Date }).IsUnique();
            e.Property(d => d.TopUpTotal).HasColumnType("decimal(18,2)");
            e.Property(d => d.TransferTotal).HasColumnType("decimal(18,2)");
        });
    }
}
