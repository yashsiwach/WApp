using Microsoft.EntityFrameworkCore;
using SharedContracts.DTOs;
using SupportTicketService.Application.DTOs;
using SupportTicketService.Application.Interfaces.Repositories;
using SupportTicketService.Application.Mappers;
using SupportTicketService.Domain.Entities;
using SupportTicketService.Infrastructure.Data;

namespace SupportTicketService.Infrastructure.Repositories;

public class SupportTicketRepository : ISupportTicketRepository
{
    private readonly SupportDbContext _db;

    public SupportTicketRepository(SupportDbContext db) => _db = db;

    public Task<SupportTicket?> FindByIdAsync(Guid id) =>
        _db.Tickets.FindAsync(id).AsTask();

    public Task<SupportTicket?> FindByIdWithRepliesAsync(Guid id) =>
        _db.Tickets.Include(t => t.Replies).FirstOrDefaultAsync(t => t.Id == id);

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

    public async Task AddAsync(SupportTicket ticket) => await _db.Tickets.AddAsync(ticket);
    public Task SaveAsync() => _db.SaveChangesAsync();
}
