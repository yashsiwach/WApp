using AdminService.Application.DTOs;

namespace AdminService.Application.Interfaces;

/// <summary>
/// Contract for retrieving aggregated admin dashboard statistics.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Returns a snapshot of current dashboard statistics including pending KYC counts and today's activity.
    /// </summary>
    Task<DashboardStatsDto> GetStatsAsync();
}
