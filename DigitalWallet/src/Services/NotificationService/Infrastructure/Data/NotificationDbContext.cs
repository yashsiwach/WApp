using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<NotificationTemplate> Templates => Set<NotificationTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── NotificationLog ──
        modelBuilder.Entity<NotificationLog>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Channel).HasMaxLength(20).IsRequired();
            e.Property(n => n.Type).HasMaxLength(50).IsRequired();
            e.Property(n => n.Recipient).HasMaxLength(256).IsRequired();
            e.Property(n => n.Subject).HasMaxLength(500);
            e.Property(n => n.Status).HasMaxLength(20).HasDefaultValue("Pending");
            e.Property(n => n.ErrorMessage).HasMaxLength(1000);
            e.HasIndex(n => new { n.UserId, n.CreatedAt });
            e.HasIndex(n => n.Status);
        });

        // ── NotificationTemplate ──
        modelBuilder.Entity<NotificationTemplate>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasIndex(t => new { t.Type, t.Channel }).IsUnique();
            e.Property(t => t.Type).HasMaxLength(50).IsRequired();
            e.Property(t => t.Channel).HasMaxLength(20).IsRequired();
            e.Property(t => t.Subject).HasMaxLength(500);
        });

        // ── Seed default templates ──
        SeedTemplates(modelBuilder);
    }

    private static void SeedTemplates(ModelBuilder modelBuilder)
    {
        var templates = new[]
        {
            new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"),
                Type = "Welcome", Channel = "Email",
                Subject = "Welcome to DigitalWallet! 🎉",
                BodyTemplate = "Hi {{Name}}, your account has been created. Start using your wallet today!",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"),
                Type = "TopUp", Channel = "Email",
                Subject = "Wallet Top-Up Successful ✅",
                BodyTemplate = "Hi {{Name}}, your wallet was topped up with ₹{{Amount}}. New balance: ₹{{Balance}}.",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"),
                Type = "Transfer", Channel = "Email",
                Subject = "Money Transfer Successful 💸",
                BodyTemplate = "Hi {{Name}}, you transferred ₹{{Amount}} successfully. New balance: ₹{{Balance}}.",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004"),
                Type = "KYCApproved", Channel = "Email",
                Subject = "KYC Verification Approved ✅",
                BodyTemplate = "Hi {{Name}}, your KYC has been approved. You can now make transfers.",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000005"),
                Type = "KYCRejected", Channel = "Email",
                Subject = "KYC Verification Rejected ❌",
                BodyTemplate = "Hi {{Name}}, your KYC was rejected. Reason: {{Reason}}. Please resubmit.",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000006"),
                Type = "PointsEarned", Channel = "Email",
                Subject = "You earned {{Points}} points! 🏆",
                BodyTemplate = "Hi {{Name}}, you earned {{Points}} points. Total balance: {{Balance}} pts. Tier: {{Tier}}.",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000007"),
                Type = "Redemption", Channel = "Email",
                Subject = "Redemption Successful 🎁",
                BodyTemplate = "Hi {{Name}}, you redeemed {{Item}} for {{Points}} points. Code: {{Code}}.",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000008"),
                Type = "PaymentFailed", Channel = "Email",
                Subject = "Payment Failed ⚠️",
                BodyTemplate = "Hi {{Name}}, your {{TransactionType}} of ₹{{Amount}} failed. Reason: {{Reason}}.",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000009"),
                Type = "TicketCreated", Channel = "Email",
                Subject = "Support Ticket {{TicketNumber}} Created",
                BodyTemplate = "Your support ticket <strong>{{TicketNumber}}</strong> has been created.<br/><br/><strong>Subject:</strong> {{Subject}}<br/><strong>Category:</strong> {{Category}}<br/><br/>Our team will respond within 24 hours. You can track your ticket in the app.",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000010"),
                Type = "TicketAdminReply", Channel = "Email",
                Subject = "New reply on your ticket {{TicketNumber}}",
                BodyTemplate = "Our support team has replied to your ticket <strong>{{TicketNumber}}</strong>.<br/><br/><strong>Subject:</strong> {{Subject}}<br/><br/><strong>Reply:</strong><br/>{{Message}}<br/><br/>You can reply or view the full conversation in the app.",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000011"),
                Type = "TicketUserReply", Channel = "Email",
                Subject = "[Ticket {{TicketNumber}}] User replied",
                BodyTemplate = "A user has replied to ticket <strong>{{TicketNumber}}</strong>.<br/><br/><strong>Subject:</strong> {{Subject}}<br/><br/><strong>Reply:</strong><br/>{{Message}}",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000012"),
                Type = "OTP", Channel = "Email",
                Subject = "Your DigitalWallet OTP Code",
                BodyTemplate = "Hi {{Name}}, your OTP is <strong>{{Code}}</strong>. It expires at {{ExpiresAt}}.",
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        modelBuilder.Entity<NotificationTemplate>().HasData(templates);
    }
}
