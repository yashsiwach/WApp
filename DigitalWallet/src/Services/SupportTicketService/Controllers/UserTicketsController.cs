using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedContracts.DTOs;
using SupportTicketService.Application.DTOs;
using SupportTicketService.Application.Interfaces;

namespace SupportTicketService.Controllers;

/// <summary>
/// Exposes authenticated user-facing REST endpoints for support ticket operations.
/// </summary>
[ApiController]
[Route("api/support/tickets")]
[Authorize]
public class UserTicketsController : ControllerBase
{
    private readonly ITicketUserService _service;

    /// <summary>
    /// Initializes the controller with the user ticket service.
    /// </summary>
    public UserTicketsController(ITicketUserService service) => _service = service;

    /// <summary>
    /// Extracts the authenticated user's identifier from JWT claims.
    /// </summary>
    private Guid GetUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(raw, out var id) ? id : throw new UnauthorizedAccessException("User identity is invalid.");
    }
    /// <summary>
    /// Extracts the authenticated user's email address from JWT claims.
    /// </summary>
    private string GetUserEmail() => User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email") ?? string.Empty;

    /// <summary>Create a new support ticket.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request)
    {
        var result = await _service.CreateAsync(GetUserId(), GetUserEmail(), request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TicketDto>.Ok(result, "Ticket created successfully."));
    }

    /// <summary>Get my tickets (paginated, filterable by status).</summary>
    [HttpGet]
    public async Task<IActionResult> GetMy(
        [FromQuery] int    page   = 1,
        [FromQuery] int    size   = 20,
        [FromQuery] string? status = null)
    {
        var result = await _service.GetMyTicketsAsync(GetUserId(), page, size, status);
        return Ok(ApiResponse<PaginatedResult<TicketSummaryDto>>.Ok(result));
    }

    /// <summary>Get a specific ticket with full reply history.</summary>
    [HttpGet("{ticketId:guid}")]
    public async Task<IActionResult> GetById(Guid ticketId)
    {
        var result = await _service.GetMyTicketByIdAsync(GetUserId(), ticketId);
        return Ok(ApiResponse<TicketDto>.Ok(result));
    }

    /// <summary>Add a reply to an open ticket.</summary>
    [HttpPost("{ticketId:guid}/replies")]
    public async Task<IActionResult> AddReply(Guid ticketId, [FromBody] AddReplyRequest request)
    {
        var result = await _service.AddReplyAsync(GetUserId(), ticketId, request);
        return Ok(ApiResponse<TicketReplyDto>.Ok(result, "Reply added."));
    }
}
