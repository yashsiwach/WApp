using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedContracts.DTOs;

namespace AdminService.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "Admin")]
/// <summary>
/// Exposes admin dashboard statistics endpoints.
/// </summary>
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _svc;
    /// <summary>
    /// Injects the dashboard service used to retrieve aggregated statistics.
    /// </summary>
    public DashboardController(IDashboardService svc) => _svc = svc;

    /// <summary>Get high-level admin dashboard statistics.</summary>
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        var result = await _svc.GetStatsAsync();
        return Ok(ApiResponse<DashboardStatsDto>.Ok(result));
    }
}
