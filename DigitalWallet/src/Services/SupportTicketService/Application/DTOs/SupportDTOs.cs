namespace SupportTicketService.Application.DTOs;

// ── Requests ──────────────────────────────────────────────

/// <summary>
/// Request payload for creating a new support ticket.
/// </summary>
public class CreateTicketRequest
{
    /// <summary>
    /// Short title describing the issue.
    /// </summary>
    public string Subject     { get; set; } = string.Empty;
    /// <summary>
    /// Detailed description of the issue.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>Low | Medium | High | Urgent</summary>
    public string Priority    { get; set; } = "Medium";
    /// <summary>Payment | Account | KYC | Rewards | Other</summary>
    public string Category    { get; set; } = "Other";
}

/// <summary>
/// Request payload for adding a reply to an existing support ticket.
/// </summary>
public class AddReplyRequest
{
    /// <summary>
    /// Text content of the reply message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request payload for an admin to update a ticket's status and optional internal note.
/// </summary>
public class UpdateTicketStatusRequest
{
    /// <summary>Open | InProgress | Resolved | Closed</summary>
    public string  Status       { get; set; } = string.Empty;
    /// <summary>
    /// Optional internal note visible only to admins.
    /// </summary>
    public string? InternalNote { get; set; }
}

// ── Responses ─────────────────────────────────────────────

/// <summary>
/// Data transfer object representing a single reply on a support ticket.
/// </summary>
public class TicketReplyDto
{
    /// <summary>
    /// Unique identifier of the reply.
    /// </summary>
    public Guid     Id         { get; set; }
    /// <summary>
    /// Identifier of the user or admin who authored the reply.
    /// </summary>
    public Guid     AuthorId   { get; set; }
    /// <summary>
    /// Role of the reply author: User or Admin.
    /// </summary>
    public string   AuthorRole { get; set; } = string.Empty;
    /// <summary>
    /// Text content of the reply.
    /// </summary>
    public string   Message    { get; set; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the reply was created.
    /// </summary>
    public DateTime CreatedAt  { get; set; }
}

/// <summary>
/// Lightweight summary DTO for listing support tickets without full reply history.
/// </summary>
public class TicketSummaryDto
{
    /// <summary>
    /// Unique identifier of the ticket.
    /// </summary>
    public Guid     Id           { get; set; }
    /// <summary>
    /// Human-readable ticket reference number, e.g. TKT-20240101-ABCD.
    /// </summary>
    public string   TicketNumber { get; set; } = string.Empty;
    /// <summary>
    /// Short title of the ticket.
    /// </summary>
    public string   Subject      { get; set; } = string.Empty;
    /// <summary>
    /// Current workflow status of the ticket.
    /// </summary>
    public string   Status       { get; set; } = string.Empty;
    /// <summary>
    /// Urgency level assigned to the ticket.
    /// </summary>
    public string   Priority     { get; set; } = string.Empty;
    /// <summary>
    /// Topic category the ticket belongs to.
    /// </summary>
    public string   Category     { get; set; } = string.Empty;
    /// <summary>
    /// Total number of replies on this ticket.
    /// </summary>
    public int      ReplyCount   { get; set; }
    /// <summary>
    /// UTC timestamp when the ticket was created.
    /// </summary>
    public DateTime CreatedAt    { get; set; }
    /// <summary>
    /// UTC timestamp of the last update to the ticket.
    /// </summary>
    public DateTime UpdatedAt    { get; set; }
}

/// <summary>
/// Full detail DTO for a support ticket, including description, admin notes, and reply list.
/// </summary>
public class TicketDto : TicketSummaryDto
{
    /// <summary>
    /// Full description of the issue provided by the user.
    /// </summary>
    public string             Description  { get; set; } = string.Empty;
    /// <summary>
    /// Internal admin-only note attached to the ticket.
    /// </summary>
    public string?            InternalNote { get; set; }
    /// <summary>
    /// UTC timestamp when the ticket was resolved or closed.
    /// </summary>
    public DateTime?          ResolvedAt   { get; set; }
    /// <summary>
    /// Chronologically ordered list of all replies on the ticket.
    /// </summary>
    public List<TicketReplyDto> Replies    { get; set; } = new();
}
