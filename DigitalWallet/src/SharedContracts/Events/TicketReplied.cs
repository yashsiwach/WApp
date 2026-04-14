namespace SharedContracts.Events;

/// <summary>
/// Event raised when a reply is posted to an existing support ticket.
/// </summary>
public class TicketReplied
{
    /// <summary>
    /// The unique identifier of the ticket that received the reply.
    /// </summary>
    public Guid   TicketId         { get; set; }
    /// <summary>
    /// The human-readable ticket reference number.
    /// </summary>
    public string TicketNumber     { get; set; } = string.Empty;
    /// <summary>"User" or "Admin"</summary>
    public string ReplyAuthorRole  { get; set; } = string.Empty;
    /// <summary>
    /// The unique identifier of the user who should receive the reply notification.
    /// </summary>
    public Guid   RecipientUserId  { get; set; }
    /// <summary>
    /// The email address of the notification recipient.
    /// </summary>
    public string RecipientEmail   { get; set; } = string.Empty;
    /// <summary>
    /// The subject of the ticket being replied to.
    /// </summary>
    public string TicketSubject    { get; set; } = string.Empty;
    /// <summary>
    /// The content of the reply message.
    /// </summary>
    public string ReplyMessage     { get; set; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the reply was posted.
    /// </summary>
    public DateTime OccurredAt     { get; set; }
}
