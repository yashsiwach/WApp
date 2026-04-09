using System.Security.Claims;
using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedContracts.DTOs;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin/kyc")]
[Authorize(Roles = "Admin")]
public class KYCController : ControllerBase
{
    private readonly IKYCManagementService _svc;
    public KYCController(IKYCManagementService svc) => _svc = svc;

    private Guid GetAdminId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    /// <summary>List all pending KYC reviews (paginated).</summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var result = await _svc.GetPendingAsync(page, size);
        return Ok(ApiResponse<PaginatedResult<KYCReviewDto>>.Ok(result));
    }

    /// <summary>Get a specific KYC review by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _svc.GetByIdAsync(id);
        return Ok(ApiResponse<KYCReviewDto>.Ok(result));
    }

    /// <summary>Approve a pending KYC review.</summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] KYCApproveRequest request)
    {
        var result = await _svc.ApproveAsync(id, GetAdminId(), request);
        return Ok(ApiResponse<KYCReviewDto>.Ok(result, "KYC approved successfully."));
    }

    /// <summary>Reject a pending KYC review.</summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] KYCRejectRequest request)
    {
        var result = await _svc.RejectAsync(id, GetAdminId(), request);
        return Ok(ApiResponse<KYCReviewDto>.Ok(result, "KYC rejected."));
    }
}
