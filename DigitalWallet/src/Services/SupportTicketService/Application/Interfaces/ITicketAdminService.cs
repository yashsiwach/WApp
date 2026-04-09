using SharedContracts.DTOs;
using SupportTicketService.Application.DTOs;

namespace SupportTicketService.Application.Interfaces;

/// <summary>Operations available only to admins.</summary>
public interface ITicketAdminService
{
    Task<PaginatedResult<TicketSummaryDto>> GetAllAsync(int page, int size, string? status, string? priority, string? category);
    Task<TicketDto>       GetByIdAsync(Guid ticketId);
    Task<TicketReplyDto>  AddAdminReplyAsync(Guid adminId, Guid ticketId, AddReplyRequest request);
    Task<TicketSummaryDto> UpdateStatusAsync(Guid adminId, Guid ticketId, UpdateTicketStatusRequest request);
}
