using PotholeApi.Models;

namespace PotholeApi.IRepositories
{
    public interface IJobTaskRepository
    {
        Task<ICollection<JobTask>> GetAllAsync();
        Task<JobTask?> GetByIdAsync(int id);
        Task<JobTask> AddAsync(JobTask jobTask);
        Task UpdateAsync(JobTask jobTask);
        Task DeleteAsync(int id);

        // Get all tasks assigned to a specific officer
        Task<ICollection<JobTask>> GetJobTasksByOfficerIdAsync(int officerId);
        Task<bool> PotholeWithTaskExistAsync(int potholeId);
    }
}
