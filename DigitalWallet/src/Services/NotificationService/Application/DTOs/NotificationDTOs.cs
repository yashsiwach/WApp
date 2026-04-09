using SharedContracts.DTOs;

namespace NotificationService.Application.DTOs;

public record NotificationLogDto
{
    public Guid Id { get; init; }
    public string Channel { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? SentAt { get; init; }
}

public record NotificationTemplateDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public string Subject { get; init; } = string.Empty;
    public string BodyTemplate { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

public record UpdateTemplateDto
{
    public string Subject { get; init; } = string.Empty;
    public string BodyTemplate { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
