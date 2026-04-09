using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedContracts.DTOs;
using SupportTicketService.Application.DTOs;
using SupportTicketService.Application.Interfaces;

namespace SupportTicketService.Controllers;

[ApiController]
[Route("api/support/admin/tickets")]
[Authorize(Roles = "Admin")]
public class AdminTicketsController : ControllerBase
{
    private readonly ITicketAdminService _service;

    public AdminTicketsController(ITicketAdminService service) => _service = service;

    private Guid GetAdminId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    /// <summary>Get all tickets with optional filters.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int     page     = 1,
        [FromQuery] int     size     = 20,
        [FromQuery] string? status   = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? category = null)
    {
        var result = await _service.GetAllAsync(page, size, status, priority, category);
        return Ok(ApiResponse<PaginatedResult<TicketSummaryDto>>.Ok(result));
    }

    /// <summary>Get any ticket with full reply history.</summary>
    [HttpGet("{ticketId:guid}")]
    public async Task<IActionResult> GetById(Guid ticketId)
    {
        var result = await _service.GetByIdAsync(ticketId);
        return Ok(ApiResponse<TicketDto>.Ok(result));
    }

    /// <summary>Add an admin reply — notifies the user by email.</summary>
    [HttpPost("{ticketId:guid}/replies")]
    public async Task<IActionResult> Reply(Guid ticketId, [FromBody] AddReplyRequest request)
    {
        var result = await _service.AddAdminReplyAsync(GetAdminId(), ticketId, request);
        return Ok(ApiResponse<TicketReplyDto>.Ok(result, "Reply sent to user."));
    }

    /// <summary>Change ticket status (Open | InProgress | Resolved | Closed).</summary>
    [HttpPatch("{ticketId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid ticketId, [FromBody] UpdateTicketStatusRequest request)
    {
        var result = await _service.UpdateStatusAsync(GetAdminId(), ticketId, request);
        return Ok(ApiResponse<TicketSummaryDto>.Ok(result, $"Ticket status updated to '{request.Status}'."));
    }
}
