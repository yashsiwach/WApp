using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using SharedContracts.DTOs;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _svc;
    public NotificationsController(INotificationService svc) => _svc = svc;

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    /// <summary>Get paginated notification history for the authenticated user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        var result = await _svc.GetLogsAsync(GetUserId(), page, size);
        return Ok(ApiResponse<PaginatedResult<NotificationLogDto>>.Ok(result));
    }

    /// <summary>Get all notification templates (Admin only).</summary>
    [HttpGet("templates")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTemplates()
    {
        var result = await _svc.GetTemplatesAsync();
        return Ok(ApiResponse<List<NotificationTemplateDto>>.Ok(result));
    }

    /// <summary>Update a notification template (Admin only).</summary>
    [HttpPut("templates/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateTemplateDto dto)
    {
        var result = await _svc.UpdateTemplateAsync(id, dto);
        return Ok(ApiResponse<NotificationTemplateDto>.Ok(result, "Template updated."));
    }
}
