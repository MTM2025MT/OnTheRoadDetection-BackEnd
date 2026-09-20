using PotholeApi.DTO;

namespace PotholeApi.Services.DashboardsServices
{
    public interface IPotholeDashboardsService
    {
        public Task<DashboardPotholeDataDto> GetPotholeDashboardsServices(int officerId);
    }
}
