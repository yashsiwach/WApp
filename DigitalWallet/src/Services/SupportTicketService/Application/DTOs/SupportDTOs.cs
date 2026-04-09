namespace SupportTicketService.Application.DTOs;

// ── Requests ──────────────────────────────────────────────

public class CreateTicketRequest
{
    public string Subject     { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    /// <summary>Low | Medium | High | Urgent</summary>
    public string Priority    { get; set; } = "Medium";
    /// <summary>Payment | Account | KYC | Rewards | Other</summary>
    public string Category    { get; set; } = "Other";
}

public class AddReplyRequest
{
    public string Message { get; set; } = string.Empty;
}

public class UpdateTicketStatusRequest
{
    /// <summary>Open | InProgress | Resolved | Closed</summary>
    public string  Status       { get; set; } = string.Empty;
    public string? InternalNote { get; set; }
}

// ── Responses ─────────────────────────────────────────────

public class TicketReplyDto
{
    public Guid     Id         { get; set; }
    public Guid     AuthorId   { get; set; }
    public string   AuthorRole { get; set; } = string.Empty;
    public string   Message    { get; set; } = string.Empty;
    public DateTime CreatedAt  { get; set; }
}

public class TicketSummaryDto
{
    public Guid     Id           { get; set; }
    public string   TicketNumber { get; set; } = string.Empty;
    public string   Subject      { get; set; } = string.Empty;
    public string   Status       { get; set; } = string.Empty;
    public string   Priority     { get; set; } = string.Empty;
    public string   Category     { get; set; } = string.Empty;
    public int      ReplyCount   { get; set; }
    public DateTime CreatedAt    { get; set; }
    public DateTime UpdatedAt    { get; set; }
}

public class TicketDto : TicketSummaryDto
{
    public string             Description  { get; set; } = string.Empty;
    public string?            InternalNote { get; set; }
    public DateTime?          ResolvedAt   { get; set; }
    public List<TicketReplyDto> Replies    { get; set; } = new();
}
