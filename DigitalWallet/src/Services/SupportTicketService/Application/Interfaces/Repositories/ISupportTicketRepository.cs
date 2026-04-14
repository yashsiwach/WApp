using SharedContracts.DTOs;
using SupportTicketService.Application.DTOs;
using SupportTicketService.Domain.Entities;

namespace SupportTicketService.Application.Interfaces.Repositories;

/// <summary>
/// Data-access contract for support ticket aggregate operations.
/// </summary>
public interface ISupportTicketRepository
{
    /// <summary>
    /// Finds a ticket by its primary key without loading replies.
    /// </summary>
    Task<SupportTicket?>                        FindByIdAsync(Guid id);
    /// <summary>
    /// Finds a ticket by its primary key and eagerly loads its replies collection.
    /// </summary>
    Task<SupportTicket?>                        FindByIdWithRepliesAsync(Guid id);
    /// <summary>
    /// Returns a paginated, optionally status-filtered list of ticket summaries for a user.
    /// </summary>
    Task<PaginatedResult<TicketSummaryDto>>     GetByUserIdPagedAsync(Guid userId, int page, int size, string? status);
    /// <summary>
    /// Returns a paginated list of all tickets with optional status, priority, and category filters.
    /// </summary>
    Task<PaginatedResult<TicketSummaryDto>>     GetAllPagedAsync(int page, int size, string? status, string? priority, string? category);
    /// <summary>
    /// Stages a new ticket entity for insertion into the database.
    /// </summary>
    Task                                        AddAsync(SupportTicket ticket);
    /// <summary>
    /// Persists all pending changes in the current database context.
    /// </summary>
    Task                                        SaveAsync();
}
