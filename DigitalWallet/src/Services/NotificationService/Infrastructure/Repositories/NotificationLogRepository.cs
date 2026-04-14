using Microsoft.EntityFrameworkCore;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces.Repositories;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;
using SharedContracts.DTOs;

namespace NotificationService.Infrastructure.Repositories;

/// <summary>
/// EF Core-backed repository for persisting and querying notification log entries.
/// </summary>
public class NotificationLogRepository : INotificationLogRepository
{
    private readonly NotificationDbContext _db;
    /// <summary>
    /// Initializes the repository with the NotificationDbContext.
    /// </summary>
    public NotificationLogRepository(NotificationDbContext db) => _db = db;

    /// <summary>
    /// Stages a new notification log entry for insertion into the database.
    /// </summary>
    public async Task AddAsync(NotificationLog log) =>
        await _db.NotificationLogs.AddAsync(log);

    /// <summary>
    /// Queries notification logs for the specified user and returns a paginated result sorted by newest first.
    /// </summary>
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

    /// <summary>
    /// Persists all pending EF Core change-tracked operations to the database.
    /// </summary>
    public Task SaveAsync() => _db.SaveChangesAsync();
}
