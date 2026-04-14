namespace SupportTicketService.Application.Interfaces.Repositories;

/// <summary>
/// Aggregates ticket and reply repositories behind a single save operation.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Repository for support ticket aggregate operations.
    /// </summary>
    ISupportTicketRepository Tickets { get; }
    /// <summary>
    /// Repository for ticket reply operations.
    /// </summary>
    ITicketReplyRepository   Replies { get; }
    /// <summary>
    /// Commits all pending changes across repositories in a single transaction.
    /// </summary>
    Task SaveAsync();
}
