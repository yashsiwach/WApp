using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using AdminService.Application.Interfaces.Repositories;

namespace AdminService.Application.Services;

public class DashboardServiceImpl : IDashboardService
{
    private readonly IActivityLogRepository _logs;

    /// <summary>Initializes dashboard service with repositories used to aggregate admin metrics.</summary>
    public DashboardServiceImpl(IActivityLogRepository logs) => _logs = logs;

    /// <summary>Builds and returns aggregated dashboard statistics for KYC, campaigns, and admin actions.</summary>
    public async Task<DashboardStatsDto> GetStatsAsync() => new()
    {
        PendingKYCCount   = await _logs.CountPendingKYCAsync(),
        ApprovedKYCToday  = await _logs.CountKYCByStatusTodayAsync("Approved"),
        RejectedKYCToday  = await _logs.CountKYCByStatusTodayAsync("Rejected"),
        AdminActionsToday = await _logs.CountAdminActionsTodayAsync()
    };
}
