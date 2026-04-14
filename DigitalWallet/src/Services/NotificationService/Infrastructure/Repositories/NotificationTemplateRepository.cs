using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces.Repositories;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories;

/// <summary>
/// EF Core-backed repository for querying and persisting notification templates.
/// </summary>
public class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly NotificationDbContext _db;
    /// <summary>
    /// Initializes the repository with the NotificationDbContext.
    /// </summary>
    public NotificationTemplateRepository(NotificationDbContext db) => _db = db;

    /// <summary>
    /// Returns the first active template matching the given type and channel, or null if none exists.
    /// </summary>
    public Task<NotificationTemplate?> FindByTypeAndChannelAsync(string type, string channel) =>
        _db.Templates.FirstOrDefaultAsync(t => t.Type == type && t.Channel == channel && t.IsActive);

    /// <summary>
    /// Retrieves all templates ordered alphabetically by type then channel.
    /// </summary>
    public Task<List<NotificationTemplate>> GetAllAsync() =>
        _db.Templates.OrderBy(t => t.Type).ThenBy(t => t.Channel).ToListAsync();

    /// <summary>
    /// Finds a template by its primary key, or returns null if not found.
    /// </summary>
    public Task<NotificationTemplate?> FindByIdAsync(Guid id) =>
        _db.Templates.FindAsync(id).AsTask();

    /// <summary>
    /// Persists all pending EF Core change-tracked operations to the database.
    /// </summary>
    public Task SaveAsync() => _db.SaveChangesAsync();
}
