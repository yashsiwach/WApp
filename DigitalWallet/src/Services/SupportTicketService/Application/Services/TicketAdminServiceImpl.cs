using MassTransit;
using SharedContracts.DTOs;
using SharedContracts.Events;
using SupportTicketService.Application.DTOs;
using SupportTicketService.Application.Interfaces;
using SupportTicketService.Application.Interfaces.Repositories;
using SupportTicketService.Application.Mappers;
using SupportTicketService.Domain.Entities;

namespace SupportTicketService.Application.Services;

public class TicketAdminServiceImpl : ITicketAdminService
{
    private readonly IUnitOfWork _uow;
    private readonly IPublishEndpoint _bus;
    private readonly ILogger<TicketAdminServiceImpl> _logger;

    /// <summary>Initializes admin ticket service dependencies for data access, messaging, and logging.</summary>
    public TicketAdminServiceImpl(IUnitOfWork uow, IPublishEndpoint bus, ILogger<TicketAdminServiceImpl> logger)
    {
        _uow    = uow;
        _bus    = bus;
        _logger = logger;
    }

    /// <summary>Returns paginated support tickets for admin view with optional status, priority, and category filters.</summary>
    public Task<PaginatedResult<TicketSummaryDto>> GetAllAsync(int page, int size, string? status, string? priority, string? category) =>
        _uow.Tickets.GetAllPagedAsync(page, size, status, priority, category);

    /// <summary>Loads a ticket with replies by id and maps it to detailed DTO for admin inspection.</summary>
    public async Task<TicketDto> GetByIdAsync(Guid ticketId)
    {
        var ticket = await _uow.Tickets.FindByIdWithRepliesAsync(ticketId)
            ?? throw new KeyNotFoundException("Ticket not found.");
        return SupportMapper.ToDetail(ticket);
    }

    /// <summary>Adds an admin reply to an open ticket, updates ticket state, and publishes reply notification event.</summary>
    public async Task<TicketReplyDto> AddAdminReplyAsync(Guid adminId, Guid ticketId, AddReplyRequest request)
    {
        var ticket = await _uow.Tickets.FindByIdWithRepliesAsync(ticketId)
            ?? throw new KeyNotFoundException("Ticket not found.");

        if (ticket.Status == "Closed")
            throw new InvalidOperationException("Cannot reply to a closed ticket.");

        var reply = new TicketReply
        {
            TicketId   = ticketId,
            AuthorId   = adminId,
            AuthorRole = "Admin",
            Message    = request.Message
        };

        if (ticket.Status == "Open")
            ticket.Status = "InProgress";

        ticket.UpdatedAt = DateTime.UtcNow;

        await _uow.Replies.AddAsync(reply);
        await _uow.SaveAsync();

        _logger.LogInformation("Admin {AdminId} replied to ticket {TicketNumber}", adminId, ticket.TicketNumber);

        await _bus.Publish(new TicketReplied
        {
            TicketId        = ticketId,
            TicketNumber    = ticket.TicketNumber,
            ReplyAuthorRole = "Admin",
            RecipientUserId = ticket.UserId,
            RecipientEmail  = ticket.UserEmail,
            TicketSubject   = ticket.Subject,
            ReplyMessage    = request.Message,
            OccurredAt      = DateTime.UtcNow
        });

        return SupportMapper.ToDto(reply);
    }

    /// <summary>Validates and updates ticket status and notes by admin, then returns the updated ticket summary.</summary>
    public async Task<TicketSummaryDto> UpdateStatusAsync(Guid adminId, Guid ticketId, UpdateTicketStatusRequest request)
    {
        var ticket = await _uow.Tickets.FindByIdWithRepliesAsync(ticketId)
            ?? throw new KeyNotFoundException("Ticket not found.");

        var validStatuses = new[] { "Open", "InProgress", "Resolved", "Closed" };
        if (!validStatuses.Contains(request.Status))
            throw new InvalidOperationException($"Invalid status '{request.Status}'. Valid values: {string.Join(", ", validStatuses)}");

        ticket.Status      = request.Status;
        ticket.UpdatedAt   = DateTime.UtcNow;
        ticket.InternalNote = request.InternalNote ?? ticket.InternalNote;

        if (request.Status is "Resolved" or "Closed")
            ticket.ResolvedAt = DateTime.UtcNow;

        await _uow.SaveAsync();

        _logger.LogInformation("Admin {AdminId} changed ticket {TicketNumber} status to {Status}", adminId, ticket.TicketNumber, request.Status);

        return SupportMapper.ToSummary(ticket);
    }
}
