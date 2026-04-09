namespace SharedContracts.Events;

public class TicketCreated
{
    public Guid   TicketId     { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public Guid   UserId       { get; set; }
    public string UserEmail    { get; set; } = string.Empty;
    public string Subject      { get; set; } = string.Empty;
    public string Category     { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
