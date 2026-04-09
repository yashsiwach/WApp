using SupportTicketService.Application.Interfaces.Repositories;
using SupportTicketService.Infrastructure.Data;

namespace SupportTicketService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SupportDbContext _db;

    public ISupportTicketRepository Tickets { get; }
    public ITicketReplyRepository   Replies { get; }

    public UnitOfWork(SupportDbContext db,
                      ISupportTicketRepository tickets,
                      ITicketReplyRepository replies)
    {
        _db     = db;
        Tickets = tickets;
        Replies = replies;
    }

    public Task SaveAsync() => _db.SaveChangesAsync();
}
