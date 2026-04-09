using AdminService.Application.DTOs;

namespace AdminService.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
