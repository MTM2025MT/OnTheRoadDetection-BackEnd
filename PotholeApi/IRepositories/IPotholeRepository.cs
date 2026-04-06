using System.Collections.ObjectModel;
using PotholeApi.Models;
namespace PotholeApi.IRepositories
{
    public interface IPotholeRepository
    {
        public Task<ICollection<Pothole>> GetAllPotholesAsync();
        public Task<Pothole?> GetPotholeByIdAsync(int id);
        public Task<ICollection<Pothole>> GetPotholesbyRange(double First_Latitude, double First_Longitude, double Second_Latitude, double Second_Longitude);
        public Task<Pothole> CreatePotholeAsync(Pothole pothole);
        public Task<Pothole?> UpdatePotholeAsync(int id, Pothole pothole);
        public Task<bool> DeletePotholeAsync(int id);
        public Task<ICollection<Pothole>> GetPotholesByDistrictIdAsync(int districtId);
        public  Task<ICollection<Pothole>> GetPotholesByNeighbourhoodIdAsync(int neighbourhoodId);
        public Task DoesExist(int id);
    }
}
