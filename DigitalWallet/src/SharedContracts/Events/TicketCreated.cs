namespace SharedContracts.Events;

/// <summary>
/// Event raised when a user submits a new support ticket.
/// </summary>
public class TicketCreated
{
    /// <summary>
    /// The unique identifier of the newly created ticket.
    /// </summary>
    public Guid   TicketId     { get; set; }
    /// <summary>
    /// The human-readable ticket reference number.
    /// </summary>
    public string TicketNumber { get; set; } = string.Empty;
    /// <summary>
    /// The unique identifier of the user who created the ticket.
    /// </summary>
    public Guid   UserId       { get; set; }
    /// <summary>
    /// The email address of the user who created the ticket.
    /// </summary>
    public string UserEmail    { get; set; } = string.Empty;
    /// <summary>
    /// The subject line of the support ticket.
    /// </summary>
    public string Subject      { get; set; } = string.Empty;
    /// <summary>
    /// The category classifying the type of support request.
    /// </summary>
    public string Category     { get; set; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the ticket was created.
    /// </summary>
    public DateTime OccurredAt { get; set; }
}
