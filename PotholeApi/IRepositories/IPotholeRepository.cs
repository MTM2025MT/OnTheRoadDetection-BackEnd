using System.Collections.ObjectModel;
using PotholeApi.Models;
using static PotholeApi.Models.Enums;
namespace PotholeApi.IRepositories
{
    public interface IPotholeRepository
    {
        public Task<ICollection<Pothole>> GetAllPotholesAsync();
        public Task<Pothole?> GetPotholeByIdAsync(int id);
        public Task<ICollection<Pothole>> GetPotholesByRange(double firstLatitude, double firstLongitude, double secondLatitude, double secondLongitude);
        public Task<Pothole> CreatePotholeAsync(Pothole pothole);
        public Task<Pothole?> UpdatePotholeAsync(int id, Pothole pothole);
        public Task<bool> DeletePotholeAsync(int id);
        public Task<ICollection<Pothole>> GetPotholesByDistrictIdAsync(int districtId);
        public  Task<ICollection<Pothole>> GetPotholesByNeighbourhoodIdAsync(int neighbourhoodId);
        public Task DoesExist(int id);
        public Task<ICollection<Pothole>> GetPotholesByFilterAsync(RepairStatus? status, Level? severity);
    }
}
