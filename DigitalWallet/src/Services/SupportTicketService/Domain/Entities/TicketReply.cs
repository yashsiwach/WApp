namespace SupportTicketService.Domain.Entities;

/// <summary>
/// Domain entity representing a single reply posted on a support ticket by a user or admin.
/// </summary>
public class TicketReply
{
    /// <summary>
    /// Unique identifier of this reply.
    /// </summary>
    public Guid   Id         { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Foreign key referencing the parent SupportTicket.
    /// </summary>
    public Guid   TicketId   { get; set; }
    /// <summary>
    /// Identifier of the user or admin who authored this reply.
    /// </summary>
    public Guid   AuthorId   { get; set; }
    /// <summary>"User" or "Admin"</summary>
    public string AuthorRole { get; set; } = string.Empty;
    /// <summary>
    /// Text content of the reply message.
    /// </summary>
    public string Message    { get; set; } = string.Empty;
    /// <summary>
    /// UTC timestamp when this reply was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property back to the parent support ticket.
    /// </summary>
    public SupportTicket Ticket { get; set; } = null!;
}
