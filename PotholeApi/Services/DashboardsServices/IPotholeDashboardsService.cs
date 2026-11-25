using PotholeApi.DTO;

namespace PotholeApi.Services.DashboardsServices
{
    public interface IPotholeDashboardsService
    {
        public Task<DashboardPotholeDataDto> GetPotholeDashboardsServices(int officerId);

        /// <summary>
        /// Recomputes the dashboard data for an officer and overwrites the cached value.
        /// Used by <see cref="DashboardCacheRefreshWorker"/> to keep the cache warm.
        /// </summary>
        public Task<DashboardPotholeDataDto> RefreshDashboardCacheAsync(int officerId);
    }
}
