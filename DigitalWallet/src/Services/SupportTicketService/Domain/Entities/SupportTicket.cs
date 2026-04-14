namespace SupportTicketService.Domain.Entities;

/// <summary>
/// Domain entity representing a customer support ticket submitted by a user.
/// </summary>
public class SupportTicket
{
    /// <summary>
    /// Unique identifier of the support ticket.
    /// </summary>
    public Guid   Id           { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Human-readable reference number, e.g. TKT-20240101-ABCD.
    /// </summary>
    public string TicketNumber { get; set; } = string.Empty; // e.g. TKT-20240101-ABCD
    /// <summary>
    /// Identifier of the user who submitted the ticket.
    /// </summary>
    public Guid   UserId       { get; set; }
    /// <summary>
    /// Email address of the submitting user, used for notifications.
    /// </summary>
    public string UserEmail    { get; set; } = string.Empty;
    /// <summary>
    /// Short title summarising the issue.
    /// </summary>
    public string Subject      { get; set; } = string.Empty;
    /// <summary>
    /// Full description of the issue provided by the user.
    /// </summary>
    public string Description  { get; set; } = string.Empty;

    /// <summary>Open | InProgress | Resolved | Closed</summary>
    public string Status   { get; set; } = "Open";
    /// <summary>Low | Medium | High | Urgent</summary>
    public string Priority { get; set; } = "Medium";
    /// <summary>Payment | Account | KYC | Rewards | Other</summary>
    public string Category { get; set; } = "Other";

    /// <summary>
    /// Internal admin-only note not visible to end users.
    /// </summary>
    public string?   InternalNote { get; set; }
    /// <summary>
    /// UTC timestamp when the ticket was first created.
    /// </summary>
    public DateTime  CreatedAt    { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp of the most recent change to the ticket.
    /// </summary>
    public DateTime  UpdatedAt    { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp when the ticket was resolved or closed; null if still open.
    /// </summary>
    public DateTime? ResolvedAt   { get; set; }

    /// <summary>
    /// Navigation property for all replies associated with this ticket.
    /// </summary>
    public List<TicketReply> Replies { get; set; } = new();
}
