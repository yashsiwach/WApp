using MassTransit;
using SharedContracts.DTOs;
using SharedContracts.Events;
using SupportTicketService.Application.DTOs;
using SupportTicketService.Application.Interfaces;
using SupportTicketService.Application.Interfaces.Repositories;
using SupportTicketService.Application.Mappers;
using SupportTicketService.Domain.Entities;

namespace SupportTicketService.Application.Services;

public class TicketUserServiceImpl : ITicketUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IPublishEndpoint _bus;
    private readonly ILogger<TicketUserServiceImpl> _logger;

    /// <summary>Initializes user ticket service dependencies for ticket persistence, messaging, and logging.</summary>
    public TicketUserServiceImpl(IUnitOfWork uow, IPublishEndpoint bus, ILogger<TicketUserServiceImpl> logger)
    {
        _uow    = uow;
        _bus    = bus;
        _logger = logger;
    }

    /// <summary>Creates a new support ticket for the user, persists it, and publishes a ticket created event.</summary>
    public async Task<TicketDto> CreateAsync(Guid userId, string userEmail, CreateTicketRequest request)
    {
        var ticket = new SupportTicket
        {
            UserId      = userId,
            UserEmail   = userEmail,
            Subject     = request.Subject,
            Description = request.Description,
            Priority    = request.Priority,
            Category    = request.Category,
            TicketNumber = GenerateTicketNumber()
        };

        await _uow.Tickets.AddAsync(ticket);
        await _uow.SaveAsync();

        _logger.LogInformation("Ticket created: {TicketNumber} by User {UserId}", ticket.TicketNumber, userId);

        await _bus.Publish(new TicketCreated
        {
            TicketId     = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            UserId       = userId,
            UserEmail    = userEmail,
            Subject      = ticket.Subject,
            Category     = ticket.Category,
            OccurredAt   = DateTime.UtcNow
        });

        return SupportMapper.ToDetail(ticket);
    }

    /// <summary>Returns paginated tickets for a user with optional status filtering.</summary>
    public Task<PaginatedResult<TicketSummaryDto>> GetMyTicketsAsync(Guid userId, int page, int size, string? status) =>
        _uow.Tickets.GetByUserIdPagedAsync(userId, page, size, status);

    /// <summary>Gets a specific user-owned ticket with replies and enforces access ownership.</summary>
    public async Task<TicketDto> GetMyTicketByIdAsync(Guid userId, Guid ticketId)
    {
        var ticket = await _uow.Tickets.FindByIdWithRepliesAsync(ticketId)
            ?? throw new KeyNotFoundException("Ticket not found.");

        if (ticket.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this ticket.");

        return SupportMapper.ToDetail(ticket);
    }

    /// <summary>Adds a user reply to an accessible non-closed ticket, updates status if needed, and publishes reply event.</summary>
    public async Task<TicketReplyDto> AddReplyAsync(Guid userId, Guid ticketId, AddReplyRequest request)
    {
        var ticket = await _uow.Tickets.FindByIdWithRepliesAsync(ticketId)
            ?? throw new KeyNotFoundException("Ticket not found.");

        if (ticket.UserId != userId)
            throw new UnauthorizedAccessException("You do not have access to this ticket.");

        if (ticket.Status == "Closed")
            throw new InvalidOperationException("Cannot reply to a closed ticket.");

        var reply = new TicketReply
        {
            TicketId   = ticketId,
            AuthorId   = userId,
            AuthorRole = "User",
            Message    = request.Message
        };

        ticket.UpdatedAt = DateTime.UtcNow;
        if (ticket.Status == "Resolved")
            ticket.Status = "Open"; // re-opens ticket when user replies after resolution

        await _uow.Replies.AddAsync(reply);
        await _uow.SaveAsync();

        _logger.LogInformation("User {UserId} replied to ticket {TicketNumber}", userId, ticket.TicketNumber);

        await _bus.Publish(new TicketReplied
        {
            TicketId        = ticketId,
            TicketNumber    = ticket.TicketNumber,
            ReplyAuthorRole = "User",
            RecipientUserId = Guid.Empty, // admin notification — no specific admin userId
            RecipientEmail  = "support@digitalwallet.com",
            TicketSubject   = ticket.Subject,
            ReplyMessage    = request.Message,
            OccurredAt      = DateTime.UtcNow
        });

        return SupportMapper.ToDto(reply);
    }

    /// <summary>Generates a unique human-readable support ticket number using current UTC date and random suffix.</summary>
    private static string GenerateTicketNumber()
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"TKT-{date}-{suffix}";
    }
}
