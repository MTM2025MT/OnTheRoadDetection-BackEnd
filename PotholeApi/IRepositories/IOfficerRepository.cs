using PotholeApi.Models;

namespace PotholeApi.IRepositories
{
    public interface IOfficerRepository
    {
        Task<ICollection<Officer>> GetAllAsync();
        Task<Officer?> GetByIdAsync(int id);
        Task AddAsync(Officer officer);
        Task UpdateAsync(Officer officer);
        Task DeleteAsync(int id);

        // Load Officer and their specific JobTasks
        Task DoesExist(int id);
        Task<Officer?> GetOfficerWithJobTasksAsync(int id);
        Task<Officer> GetOfficerByUserIdAsync(string userId);
    }
}
