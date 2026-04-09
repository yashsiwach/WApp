namespace SharedContracts.Events;

public class TicketReplied
{
    public Guid   TicketId         { get; set; }
    public string TicketNumber     { get; set; } = string.Empty;
    /// <summary>"User" or "Admin"</summary>
    public string ReplyAuthorRole  { get; set; } = string.Empty;
    public Guid   RecipientUserId  { get; set; }
    public string RecipientEmail   { get; set; } = string.Empty;
    public string TicketSubject    { get; set; } = string.Empty;
    public string ReplyMessage     { get; set; } = string.Empty;
    public DateTime OccurredAt     { get; set; }
}
