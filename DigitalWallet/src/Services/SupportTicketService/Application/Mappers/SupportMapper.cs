using SupportTicketService.Application.DTOs;
using SupportTicketService.Domain.Entities;

namespace SupportTicketService.Application.Mappers;

/// <summary>
/// Provides static mapping methods between domain entities and their DTO representations.
/// </summary>
public static class SupportMapper
{
    /// <summary>
    /// Maps a TicketReply entity to its corresponding TicketReplyDto.
    /// </summary>
    public static TicketReplyDto ToDto(TicketReply r) => new()
    {
        Id         = r.Id,
        AuthorId   = r.AuthorId,
        AuthorRole = r.AuthorRole,
        Message    = r.Message,
        CreatedAt  = r.CreatedAt
    };

    /// <summary>
    /// Maps a SupportTicket entity to a lightweight TicketSummaryDto without reply details.
    /// </summary>
    public static TicketSummaryDto ToSummary(SupportTicket t) => new()
    {
        Id           = t.Id,
        TicketNumber = t.TicketNumber,
        Subject      = t.Subject,
        Status       = t.Status,
        Priority     = t.Priority,
        Category     = t.Category,
        ReplyCount   = t.Replies.Count,
        CreatedAt    = t.CreatedAt,
        UpdatedAt    = t.UpdatedAt
    };

    /// <summary>
    /// Maps a SupportTicket entity to a full TicketDto including all ordered replies.
    /// </summary>
    public static TicketDto ToDetail(SupportTicket t) => new()
    {
        Id           = t.Id,
        TicketNumber = t.TicketNumber,
        Subject      = t.Subject,
        Description  = t.Description,
        Status       = t.Status,
        Priority     = t.Priority,
        Category     = t.Category,
        InternalNote = t.InternalNote,
        ReplyCount   = t.Replies.Count,
        CreatedAt    = t.CreatedAt,
        UpdatedAt    = t.UpdatedAt,
        ResolvedAt   = t.ResolvedAt,
        Replies      = t.Replies.OrderBy(r => r.CreatedAt).Select(ToDto).ToList()
    };
}
