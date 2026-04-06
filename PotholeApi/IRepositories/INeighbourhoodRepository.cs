using PotholeApi.Models;
namespace PotholeApi.IRepositories
{
    public interface INeighbourhoodRepository
    {
        Task<ICollection<Neighbourhood>> GetAllAsync();
        Task<Neighbourhood?> GetByIdAsync(int id);
        Task<ICollection<Neighbourhood>> GetNeighbourhoodsByDistrictIdAsync(int districtId);
        Task<ICollection<Neighbourhood>> GetNeighbourhoodsByDistrictIdWithPotholesAsync(int districtId);
    }
}
