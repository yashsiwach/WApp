using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AuthService.Infrastructure.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    // Suppress: "KYCDocument/RefreshToken has a required navigation to User which has a
    // global query filter (soft-delete). Rows referencing a soft-deleted User will have
    // a null navigation even though the FK is non-nullable."
    // This is acceptable in our design — queries that need deleted users use IgnoreQueryFilters().
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning));

    public DbSet<User> Users => Set<User>();
    public DbSet<OTPLog> OTPLogs => Set<OTPLog>();
    public DbSet<KYCDocument> KYCDocuments => Set<KYCDocument>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Phone).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.Phone).HasMaxLength(20).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("User");
            e.HasQueryFilter(u => !u.IsDeleted); // soft-delete filter
        });

        // ── OTPLog ──
        modelBuilder.Entity<OTPLog>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Email).HasMaxLength(256).IsRequired();
            e.Property(o => o.Code).HasMaxLength(10).IsRequired();
            e.HasOne(o => o.User)
             .WithMany(u => u.OTPLogs)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── KYCDocument ──
        modelBuilder.Entity<KYCDocument>(e =>
        {
            e.HasKey(k => k.Id);
            e.Property(k => k.DocType).HasMaxLength(50).IsRequired();
            e.Property(k => k.FileUrl).HasMaxLength(500).IsRequired();
            e.Property(k => k.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.Property(k => k.ReviewNotes).HasMaxLength(1000);
            e.HasOne(k => k.User)
             .WithMany(u => u.KYCDocuments)
             .HasForeignKey(k => k.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── RefreshToken ──
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.HasIndex(r => r.Token).IsUnique();
            e.Property(r => r.Token).HasMaxLength(512).IsRequired();
            e.HasOne(r => r.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
