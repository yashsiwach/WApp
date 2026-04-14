using SharedContracts.DTOs;
using SupportTicketService.Application.DTOs;

namespace SupportTicketService.Application.Interfaces;

/// <summary>Operations available to the authenticated end-user.</summary>
public interface ITicketUserService
{
    /// <summary>
    /// Creates a new support ticket for the given user and publishes a ticket-created event.
    /// </summary>
    Task<TicketDto>                        CreateAsync(Guid userId, string userEmail, CreateTicketRequest request);
    /// <summary>
    /// Returns a paginated list of tickets owned by the specified user with optional status filter.
    /// </summary>
    Task<PaginatedResult<TicketSummaryDto>> GetMyTicketsAsync(Guid userId, int page, int size, string? status);
    /// <summary>
    /// Retrieves a specific ticket with replies, enforcing that it belongs to the requesting user.
    /// </summary>
    Task<TicketDto>                        GetMyTicketByIdAsync(Guid userId, Guid ticketId);
    /// <summary>
    /// Adds a user reply to the specified ticket and publishes a reply notification event.
    /// </summary>
    Task<TicketReplyDto>                   AddReplyAsync(Guid userId, Guid ticketId, AddReplyRequest request);
}
