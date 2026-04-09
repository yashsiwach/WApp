using Microsoft.EntityFrameworkCore;
using SupportTicketService.Domain.Entities;

namespace SupportTicketService.Infrastructure.Data;

public class SupportDbContext : DbContext
{
    public SupportDbContext(DbContextOptions<SupportDbContext> options) : base(options) { }

    public DbSet<SupportTicket> Tickets => Set<SupportTicket>();
    public DbSet<TicketReply>   Replies => Set<TicketReply>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SupportTicket>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.TicketNumber).HasMaxLength(30).IsRequired();
            e.HasIndex(t => t.TicketNumber).IsUnique();
            e.Property(t => t.UserEmail).HasMaxLength(256).IsRequired();
            e.Property(t => t.Subject).HasMaxLength(300).IsRequired();
            e.Property(t => t.Status).HasMaxLength(20).HasDefaultValue("Open");
            e.Property(t => t.Priority).HasMaxLength(20).HasDefaultValue("Medium");
            e.Property(t => t.Category).HasMaxLength(50).HasDefaultValue("Other");
            e.Property(t => t.InternalNote).HasMaxLength(1000);
            e.HasIndex(t => t.UserId);
            e.HasIndex(t => t.Status);
            e.HasIndex(t => new { t.Status, t.Priority });

            e.HasMany(t => t.Replies)
             .WithOne(r => r.Ticket)
             .HasForeignKey(r => r.TicketId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TicketReply>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.AuthorRole).HasMaxLength(20).IsRequired();
            e.Property(r => r.Message).HasMaxLength(4000).IsRequired();
            e.HasIndex(r => r.TicketId);
        });
    }
}
