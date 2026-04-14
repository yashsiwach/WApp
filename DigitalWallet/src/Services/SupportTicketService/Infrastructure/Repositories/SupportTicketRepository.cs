using Microsoft.EntityFrameworkCore;
using SharedContracts.DTOs;
using SupportTicketService.Application.DTOs;
using SupportTicketService.Application.Interfaces.Repositories;
using SupportTicketService.Application.Mappers;
using SupportTicketService.Domain.Entities;
using SupportTicketService.Infrastructure.Data;

namespace SupportTicketService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ISupportTicketRepository for querying and persisting support tickets.
/// </summary>
public class SupportTicketRepository : ISupportTicketRepository
{
    private readonly SupportDbContext _db;

    /// <summary>
    /// Initializes the repository with the shared database context.
    /// </summary>
    public SupportTicketRepository(SupportDbContext db) => _db = db;

    /// <summary>
    /// Finds a ticket by primary key without loading its replies.
    /// </summary>
    public Task<SupportTicket?> FindByIdAsync(Guid id) =>
        _db.Tickets.FindAsync(id).AsTask();

    /// <summary>
    /// Finds a ticket by primary key and eagerly includes its replies collection.
    /// </summary>
    public Task<SupportTicket?> FindByIdWithRepliesAsync(Guid id) =>
        _db.Tickets.Include(t => t.Replies).FirstOrDefaultAsync(t => t.Id == id);

    /// <summary>
    /// Queries tickets belonging to a user, applies optional status filter, and returns a paged summary result.
    /// </summary>
    public async Task<PaginatedResult<TicketSummaryDto>> GetByUserIdPagedAsync(Guid userId, int page, int size, string? status)
    {
        var q = _db.Tickets.Include(t => t.Replies)
                   .Where(t => t.UserId == userId);

        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(t => t.Status == status);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(t => t.CreatedAt)
                           .Skip((page - 1) * size).Take(size)
                           .Select(t => SupportMapper.ToSummary(t))
                           .ToListAsync();

        return new PaginatedResult<TicketSummaryDto> { Items = items, TotalCount = total, Page = page, PageSize = size };
    }

    /// <summary>
    /// Queries all tickets with optional status, priority, and category filters and returns a paged summary result.
    /// </summary>
    public async Task<PaginatedResult<TicketSummaryDto>> GetAllPagedAsync(int page, int size, string? status, string? priority, string? category)
    {
        var q = _db.Tickets.Include(t => t.Replies).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))   q = q.Where(t => t.Status   == status);
        if (!string.IsNullOrWhiteSpace(priority))  q = q.Where(t => t.Priority == priority);
        if (!string.IsNullOrWhiteSpace(category))  q = q.Where(t => t.Category == category);

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(t => t.CreatedAt)
                           .Skip((page - 1) * size).Take(size)
                           .Select(t => SupportMapper.ToSummary(t))
                           .ToListAsync();

        return new PaginatedResult<TicketSummaryDto> { Items = items, TotalCount = total, Page = page, PageSize = size };
    }

    /// <summary>
    /// Stages the given ticket entity for insertion into the Tickets table.
    /// </summary>
    public async Task AddAsync(SupportTicket ticket) => await _db.Tickets.AddAsync(ticket);
    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    public Task SaveAsync() => _db.SaveChangesAsync();
}
