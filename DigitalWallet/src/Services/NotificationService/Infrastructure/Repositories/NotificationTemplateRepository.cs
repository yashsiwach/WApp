using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces.Repositories;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly NotificationDbContext _db;
    public NotificationTemplateRepository(NotificationDbContext db) => _db = db;

    public Task<NotificationTemplate?> FindByTypeAndChannelAsync(string type, string channel) =>
        _db.Templates.FirstOrDefaultAsync(t => t.Type == type && t.Channel == channel && t.IsActive);

    public Task<List<NotificationTemplate>> GetAllAsync() =>
        _db.Templates.OrderBy(t => t.Type).ThenBy(t => t.Channel).ToListAsync();

    public Task<NotificationTemplate?> FindByIdAsync(Guid id) =>
        _db.Templates.FindAsync(id).AsTask();

    public Task SaveAsync() => _db.SaveChangesAsync();
}
