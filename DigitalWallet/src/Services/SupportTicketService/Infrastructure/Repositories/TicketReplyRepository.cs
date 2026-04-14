using SupportTicketService.Application.Interfaces.Repositories;
using SupportTicketService.Domain.Entities;
using SupportTicketService.Infrastructure.Data;

namespace SupportTicketService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ITicketReplyRepository for persisting ticket replies.
/// </summary>
public class TicketReplyRepository : ITicketReplyRepository
{
    private readonly SupportDbContext _db;

    /// <summary>
    /// Initializes the repository with the shared database context.
    /// </summary>
    public TicketReplyRepository(SupportDbContext db) => _db = db;

    /// <summary>
    /// Stages the given reply entity for insertion into the Replies table.
    /// </summary>
    public async Task AddAsync(TicketReply reply) => await _db.Replies.AddAsync(reply);
    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    public Task SaveAsync() => _db.SaveChangesAsync();
}
