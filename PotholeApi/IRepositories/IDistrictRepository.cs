using PotholeApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PotholeApi.IRepositories
{
    public interface IDistrictRepository
    {
        // Get all districts (for listing or map overview)
        Task<IEnumerable<District>> GetAllAsync();

        // Get a specific district by ID
        Task<District?> GetByIdAsync(int id);
        Task<List<DistrictJustText>> GetDistrictNamesAsync();

        // Add other methods here later if needed (e.g., GetByLocation)
    }
}