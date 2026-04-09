using NotificationService.Application.DTOs;
using NotificationService.Domain.Entities;
using SharedContracts.DTOs;

namespace NotificationService.Application.Interfaces.Repositories;

public interface INotificationLogRepository
{
    Task AddAsync(NotificationLog log);
    Task<PaginatedResult<NotificationLogDto>> GetPagedByUserIdAsync(Guid userId, int page, int size);
    Task SaveAsync();
}
