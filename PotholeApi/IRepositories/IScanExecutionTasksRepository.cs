using System.Collections.Generic;
using PotholeApi.Models;
namespace PotholeApi.IRepositories
{
    public interface IScanExecutionTasksRepository
    {
        Task<IEnumerable<ScanExecutionTask>> GetAllAsync();
        Task<ScanExecutionTask> GetByIdAsync(int id);
        Task AddAsync(ScanExecutionTask scanExecutionTask);
        Task UpdateAsync(ScanExecutionTask scanExecutionTask);
        Task DeleteAsync(int id);
    }
}
