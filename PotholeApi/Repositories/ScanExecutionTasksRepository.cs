using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PotholeApi.IRepositories;
using PotholeApi.Models;

namespace PotholeApi.Repositories
{
    public class ScanExecutionTasksRepository : IScanExecutionTasksRepository
    {
        private readonly Context _context;
        private readonly ILogger<ScanExecutionTasksRepository> _logger;

        public ScanExecutionTasksRepository(Context context, ILogger<ScanExecutionTasksRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ScanExecutionTask>> GetAllAsync()
        {
            return await _context.ScanExecutionTasks
                .Include(s => s.RoutePoints)
                .Include(s => s.RoutePath)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ScanExecutionTask?> GetByIdAsync(int id)
        {
            return await _context.ScanExecutionTasks
                .Include(s => s.RoutePoints)
                .Include(s => s.RoutePath)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task AddAsync(ScanExecutionTask scanExecutionTask)
        {
            await _context.ScanExecutionTasks.AddAsync(scanExecutionTask);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created scan execution task {ScanExecutionTaskId}", scanExecutionTask.Id);
        }

        public async Task UpdateAsync(ScanExecutionTask scanExecutionTask)
        {
            _context.ScanExecutionTasks.Update(scanExecutionTask);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated scan execution task {ScanExecutionTaskId}", scanExecutionTask.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _context.ScanExecutionTasks.FindAsync(id);
            if (task != null)
            {
                _context.ScanExecutionTasks.Remove(task);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted scan execution task {ScanExecutionTaskId}", id);
            }
            else
            {
                _logger.LogWarning("DeleteAsync: scan execution task {ScanExecutionTaskId} not found", id);
            }
        }
    }
}
