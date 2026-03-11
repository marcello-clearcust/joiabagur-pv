using JoiabagurPV.Application.DTOs.Dashboard;

namespace JoiabagurPV.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetGlobalStatsAsync();
    Task<DashboardStatsDto> GetPosStatsAsync(Guid posId, Guid userId, bool isAdmin = false);
}
