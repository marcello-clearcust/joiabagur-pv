using JoiabagurPV.Application.DTOs.Dashboard;

namespace JoiabagurPV.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetGlobalStatsAsync();
    Task<DashboardStatsDto> GetPosStatsAsync(Guid posId, Guid userId, bool isAdmin = false);
    Task<PaginatedLowStockResult> GetLowStockAsync(int page, int pageSize, int maxQuantity = 2);

    /// <summary>
    /// Removes all cached dashboard aggregations (payment distribution, return category distribution).
    /// Must be called whenever a sale or return is created or modified.
    /// </summary>
    void InvalidateDashboardCache();
}
