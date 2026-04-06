using PotholeApi.Models;

namespace PotholeApi.IRepositories
{
    public interface IRepairTeamRepository
    {
        // Standard CRUD
        Task<IEnumerable<RepairTeam>> GetAllAsync();
        Task<RepairTeam?> GetByIdAsync(int id);
        Task AddAsync(RepairTeam repairTeam);
        Task UpdateAsync(RepairTeam repairTeam);
        Task DeleteAsync(int id);
        Task DoesExist(int id);

        // Special: Get the team and include their list of assigned JobTasks
        Task<RepairTeam?> GetRepairTeamWithTasksAsync(int id);
    }

}
