using Microsoft.EntityFrameworkCore;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces.Repositories;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;
using SharedContracts.DTOs;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationLogRepository : INotificationLogRepository
{
    private readonly NotificationDbContext _db;
    public NotificationLogRepository(NotificationDbContext db) => _db = db;

    public async Task AddAsync(NotificationLog log) =>
        await _db.NotificationLogs.AddAsync(log);

    public async Task<PaginatedResult<NotificationLogDto>> GetPagedByUserIdAsync(Guid userId, int page, int size)
    {
        var query = _db.NotificationLogs.Where(n => n.UserId == userId);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(n => new NotificationLogDto
            {
                Id        = n.Id,
                Channel   = n.Channel,
                Type      = n.Type,
                Subject   = n.Subject,
                Status    = n.Status,
                CreatedAt = n.CreatedAt,
                SentAt    = n.SentAt
            })
            .ToListAsync();

        return new PaginatedResult<NotificationLogDto> { Items = items, Page = page, PageSize = size, TotalCount = total };
    }

    public Task SaveAsync() => _db.SaveChangesAsync();
}
