namespace SupportTicketService.Domain.Entities;

public class SupportTicket
{
    public Guid   Id           { get; set; } = Guid.NewGuid();
    public string TicketNumber { get; set; } = string.Empty; // e.g. TKT-20240101-ABCD
    public Guid   UserId       { get; set; }
    public string UserEmail    { get; set; } = string.Empty;
    public string Subject      { get; set; } = string.Empty;
    public string Description  { get; set; } = string.Empty;

    /// <summary>Open | InProgress | Resolved | Closed</summary>
    public string Status   { get; set; } = "Open";
    /// <summary>Low | Medium | High | Urgent</summary>
    public string Priority { get; set; } = "Medium";
    /// <summary>Payment | Account | KYC | Rewards | Other</summary>
    public string Category { get; set; } = "Other";

    public string?   InternalNote { get; set; }
    public DateTime  CreatedAt    { get; set; } = DateTime.UtcNow;
    public DateTime  UpdatedAt    { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt   { get; set; }

    public List<TicketReply> Replies { get; set; } = new();
}
