using NotificationService.Application.DTOs;
using SharedContracts.DTOs;

namespace NotificationService.Application.Interfaces;

public interface INotificationService
{
    /// <summary>Send a notification from a template, persisting the log.</summary>
    Task SendAsync(Guid userId, string channel, string type, string recipient, Dictionary<string, string> placeholders);

    /// <summary>Get paginated notification history for a user.</summary>
    Task<PaginatedResult<NotificationLogDto>> GetLogsAsync(Guid userId, int page, int size);

    /// <summary>Get all templates (for admin management).</summary>
    Task<List<NotificationTemplateDto>> GetTemplatesAsync();

    /// <summary>Update a template by ID.</summary>
    Task<NotificationTemplateDto> UpdateTemplateAsync(Guid templateId, UpdateTemplateDto dto);
}
