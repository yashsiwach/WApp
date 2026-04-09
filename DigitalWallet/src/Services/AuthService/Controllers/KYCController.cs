using System.Security.Claims;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedContracts.DTOs;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth/kyc")]
[Authorize]
public class KYCController : ControllerBase
{
    private readonly IKYCService _kycService;

    public KYCController(IKYCService kycService)
    {
        _kycService = kycService;
    }

    /// <summary>
    /// Submit a KYC document for verification.
    /// </summary>
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] KYCSubmitRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)?? User.FindFirstValue("sub")!);
        var result = await _kycService.SubmitAsync(userId, request);
        return Ok(ApiResponse<KYCStatusResponse>.Ok(result, "KYC document submitted."));
    }

    /// <summary>
    /// Get the status of all KYC submissions for the current user.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)?? User.FindFirstValue("sub")!);
        var result = await _kycService.GetStatusAsync(userId);
        return Ok(ApiResponse<List<KYCStatusResponse>>.Ok(result));
    }
}
