using SharedContracts.DTOs;
using SupportTicketService.Application.DTOs;

namespace SupportTicketService.Application.Interfaces;

/// <summary>Operations available only to admins.</summary>
public interface ITicketAdminService
{
    /// <summary>
    /// Returns a paginated list of all tickets with optional status, priority, and category filters.
    /// </summary>
    Task<PaginatedResult<TicketSummaryDto>> GetAllAsync(int page, int size, string? status, string? priority, string? category);
    /// <summary>
    /// Retrieves full ticket details including all replies by ticket id.
    /// </summary>
    Task<TicketDto>       GetByIdAsync(Guid ticketId);
    /// <summary>
    /// Appends an admin reply to the specified ticket and triggers a user notification event.
    /// </summary>
    Task<TicketReplyDto>  AddAdminReplyAsync(Guid adminId, Guid ticketId, AddReplyRequest request);
    /// <summary>
    /// Updates the workflow status and optional internal note of a ticket.
    /// </summary>
    Task<TicketSummaryDto> UpdateStatusAsync(Guid adminId, Guid ticketId, UpdateTicketStatusRequest request);
}
