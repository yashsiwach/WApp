using SupportTicketService.Application.DTOs;
using SupportTicketService.Domain.Entities;

namespace SupportTicketService.Application.Mappers;

public static class SupportMapper
{
    public static TicketReplyDto ToDto(TicketReply r) => new()
    {
        Id         = r.Id,
        AuthorId   = r.AuthorId,
        AuthorRole = r.AuthorRole,
        Message    = r.Message,
        CreatedAt  = r.CreatedAt
    };

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
