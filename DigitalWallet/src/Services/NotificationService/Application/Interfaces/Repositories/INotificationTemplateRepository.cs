using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces.Repositories;

/// <summary>
/// Persistence contract for looking up and persisting notification templates.
/// </summary>
public interface INotificationTemplateRepository
{
    /// <summary>
    /// Finds the active template matching the given notification type and delivery channel.
    /// </summary>
    Task<NotificationTemplate?> FindByTypeAndChannelAsync(string type, string channel);
    /// <summary>
    /// Retrieves all notification templates ordered by type and channel.
    /// </summary>
    Task<List<NotificationTemplate>> GetAllAsync();
    /// <summary>
    /// Retrieves a single template by its unique identifier, or null if not found.
    /// </summary>
    Task<NotificationTemplate?> FindByIdAsync(Guid id);
    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    Task SaveAsync();
}
