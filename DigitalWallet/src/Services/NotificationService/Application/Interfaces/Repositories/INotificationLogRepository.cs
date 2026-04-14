using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;
using SharedContracts.DTOs;

namespace NotificationService.Application.Interfaces.Repositories;

/// <summary>
/// Persistence contract for creating and querying notification log entries.
/// </summary>
public interface INotificationLogRepository
{
    /// <summary>
    /// Stages a new notification log entry for insertion into the data store.
    /// </summary>
    Task AddAsync(NotificationLog log);
    /// <summary>
    /// Returns a paginated list of notification logs belonging to the specified user.
    /// </summary>
    Task<PaginatedResult<NotificationLogDto>> GetPagedByUserIdAsync(Guid userId, int page, int size);
    /// <summary>
    /// Persists all pending changes to the underlying data store.
    /// </summary>
    Task SaveAsync();
}
