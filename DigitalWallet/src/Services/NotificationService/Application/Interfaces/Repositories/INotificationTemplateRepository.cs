using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces.Repositories;

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> FindByTypeAndChannelAsync(string type, string channel);
    Task<List<NotificationTemplate>> GetAllAsync();
    Task<NotificationTemplate?> FindByIdAsync(Guid id);
    Task SaveAsync();
}
