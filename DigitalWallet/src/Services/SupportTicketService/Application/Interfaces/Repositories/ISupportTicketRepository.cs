using SharedContracts.DTOs;
using SupportTicketService.Application.DTOs;
using SupportTicketService.Domain.Entities;

namespace SupportTicketService.Application.Interfaces.Repositories;

public interface ISupportTicketRepository
{
    Task<SupportTicket?>                        FindByIdAsync(Guid id);
    Task<SupportTicket?>                        FindByIdWithRepliesAsync(Guid id);
    Task<PaginatedResult<TicketSummaryDto>>     GetByUserIdPagedAsync(Guid userId, int page, int size, string? status);
    Task<PaginatedResult<TicketSummaryDto>>     GetAllPagedAsync(int page, int size, string? status, string? priority, string? category);
    Task                                        AddAsync(SupportTicket ticket);
    Task                                        SaveAsync();
}
