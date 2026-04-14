using SupportTicketService.Application.Interfaces.Repositories;
using SupportTicketService.Infrastructure.Data;

namespace SupportTicketService.Infrastructure.Repositories;

/// <summary>
/// Concrete unit-of-work that groups ticket and reply repositories and coordinates a shared save operation.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly SupportDbContext _db;

    /// <summary>
    /// Repository for support ticket aggregate operations.
    /// </summary>
    public ISupportTicketRepository Tickets { get; }
    /// <summary>
    /// Repository for ticket reply operations.
    /// </summary>
    public ITicketReplyRepository   Replies { get; }

    /// <summary>
    /// Initializes the unit of work with the database context and injected repository instances.
    /// </summary>
    public UnitOfWork(SupportDbContext db,ISupportTicketRepository tickets,ITicketReplyRepository replies)
    {
        _db     = db;
        Tickets = tickets;
        Replies = replies;
    }

    /// <summary>
    /// Commits all pending changes across repositories to the database.
    /// </summary>
    public Task SaveAsync() => _db.SaveChangesAsync();
}
