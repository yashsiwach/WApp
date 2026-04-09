namespace SupportTicketService.Domain.Entities;

public class TicketReply
{
    public Guid   Id         { get; set; } = Guid.NewGuid();
    public Guid   TicketId   { get; set; }
    public Guid   AuthorId   { get; set; }
    /// <summary>"User" or "Admin"</summary>
    public string AuthorRole { get; set; } = string.Empty;
    public string Message    { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public SupportTicket Ticket { get; set; } = null!;
}
