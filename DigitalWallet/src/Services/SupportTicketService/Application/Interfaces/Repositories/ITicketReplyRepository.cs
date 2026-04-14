using SupportTicketService.Domain.Entities;

namespace SupportTicketService.Application.Interfaces.Repositories;

/// <summary>
/// Data-access contract for ticket reply persistence operations.
/// </summary>
public interface ITicketReplyRepository
{
    /// <summary>
    /// Stages a new reply entity for insertion into the database.
    /// </summary>
    Task AddAsync(TicketReply reply);
    /// <summary>
    /// Persists all pending changes in the current database context.
    /// </summary>
    Task SaveAsync();
}
