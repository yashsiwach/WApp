using SharedContracts.DTOs;
using SupportTicketService.Application.DTOs;

namespace SupportTicketService.Application.Interfaces;

/// <summary>Operations available to the authenticated end-user.</summary>
public interface ITicketUserService
{
    Task<TicketDto>                        CreateAsync(Guid userId, string userEmail, CreateTicketRequest request);
    Task<PaginatedResult<TicketSummaryDto>> GetMyTicketsAsync(Guid userId, int page, int size, string? status);
    Task<TicketDto>                        GetMyTicketByIdAsync(Guid userId, Guid ticketId);
    Task<TicketReplyDto>                   AddReplyAsync(Guid userId, Guid ticketId, AddReplyRequest request);
}
