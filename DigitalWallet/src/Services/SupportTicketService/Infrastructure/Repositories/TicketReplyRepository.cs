using SupportTicketService.Application.Interfaces.Repositories;
using SupportTicketService.Domain.Entities;
using SupportTicketService.Infrastructure.Data;

namespace SupportTicketService.Infrastructure.Repositories;

public class TicketReplyRepository : ITicketReplyRepository
{
    private readonly SupportDbContext _db;

    public TicketReplyRepository(SupportDbContext db) => _db = db;

    public async Task AddAsync(TicketReply reply) => await _db.Replies.AddAsync(reply);
    public Task SaveAsync() => _db.SaveChangesAsync();
}
