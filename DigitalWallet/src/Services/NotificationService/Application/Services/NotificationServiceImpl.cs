using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Interfaces.Repositories;
using NotificationService.Domain.Entities;
using SharedContracts.DTOs;

namespace NotificationService.Application.Services;

public class NotificationServiceImpl : INotificationService
{
    private readonly INotificationLogRepository      _logs;
    private readonly INotificationTemplateRepository _templates;
    private readonly IEmailSender                    _emailSender;
    private readonly ILogger<NotificationServiceImpl> _logger;

    /// <summary>Initializes notification dependencies for templates, delivery, logging, and persisted notification history.</summary>
    public NotificationServiceImpl(
        INotificationLogRepository logs,
        INotificationTemplateRepository templates,
        IEmailSender emailSender,
        ILogger<NotificationServiceImpl> logger)
    {
        _logs        = logs;
        _templates   = templates;
        _emailSender = emailSender;
        _logger      = logger;
    }

    /// <summary>Builds notification content from templates and placeholders, sends through channel, and persists send status.</summary>
    public async Task SendAsync(Guid userId, string channel, string type, string recipient, Dictionary<string, string> placeholders)
    {
        var template = await _templates.FindByTypeAndChannelAsync(type, channel);

        string subject = template?.Subject ?? $"DigitalWallet – {type}";
        string body = template?.BodyTemplate ?? BuildFallbackBody(type, placeholders);

        foreach (var (key, value) in placeholders)
        {
            var safeValue = System.Net.WebUtility.HtmlEncode(value);
            subject = subject.Replace($"{{{{{key}}}}}", safeValue);
            body    = body.Replace($"{{{{{key}}}}}", safeValue);
        }

        var log = new NotificationLog
        {
            UserId    = userId,
            Channel   = channel,
            Type      = type,
            Recipient = recipient,
            Subject   = subject,
            Body      = body,
            Status    = "Pending"
        };
        await _logs.AddAsync(log);
        await _logs.SaveAsync();

        try
        {
            if (channel.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                var displayName = placeholders.TryGetValue("Name", out var n) ? n : recipient;
                await _emailSender.SendAsync(recipient, displayName, subject, body);
            }
            else
            {
                // SMS / Push: pluggable — add more IXxxSender implementations and route here
                _logger.LogWarning("[{Channel}] No sender configured for channel. Skipping.", channel);
            }

            log.Status = "Sent";
            log.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            log.Status       = "Failed";
            log.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Failed to send {Type} {Channel} notification to {Recipient}", type, channel, recipient);
        }

        await _logs.SaveAsync();
    }

    /// <summary>Returns paginated notification logs for a specific user.</summary>
    public Task<PaginatedResult<NotificationLogDto>> GetLogsAsync(Guid userId, int page, int size) =>
        _logs.GetPagedByUserIdAsync(userId, page, size);

    /// <summary>Retrieves all notification templates and maps them to DTOs.</summary>
    public async Task<List<NotificationTemplateDto>> GetTemplatesAsync()
    {
        var all = await _templates.GetAllAsync();
        return all.Select(MapTemplate).ToList();
    }

    /// <summary>Updates an existing notification template fields and returns the mapped updated template.</summary>
    public async Task<NotificationTemplateDto> UpdateTemplateAsync(Guid templateId, UpdateTemplateDto dto)
    {
        var template = await _templates.FindByIdAsync(templateId)
            ?? throw new KeyNotFoundException("Template not found.");

        template.Subject      = dto.Subject;
        template.BodyTemplate = dto.BodyTemplate;
        template.IsActive     = dto.IsActive;
        template.UpdatedAt    = DateTime.UtcNow;

        await _templates.SaveAsync();
        return MapTemplate(template);
    }

    /// <summary>Maps a notification template domain entity to its DTO representation.</summary>
    private static NotificationTemplateDto MapTemplate(Domain.Entities.NotificationTemplate t) => new()
    {
        Id           = t.Id,
        Type         = t.Type,
        Channel      = t.Channel,
        Subject      = t.Subject,
        BodyTemplate = t.BodyTemplate,
        IsActive     = t.IsActive
    };

    private static string BuildFallbackBody(string type, Dictionary<string, string> placeholders)
    {
        if (type.Equals("OTP", StringComparison.OrdinalIgnoreCase) && placeholders.TryGetValue("Code", out var code))
            return $"Your OTP code is <strong>{System.Net.WebUtility.HtmlEncode(code)}</strong>.";

        return $"Notification type: {type}";
    }
}
