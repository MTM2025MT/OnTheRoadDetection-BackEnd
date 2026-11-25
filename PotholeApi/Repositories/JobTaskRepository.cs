using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PotholeApi.IRepositories;
using PotholeApi.Models;

using System.Collections.ObjectModel;

namespace PotholeApi.Repositories
{
    public class JobTaskRepository : IJobTaskRepository
    {
        private readonly Context _context;
        private readonly ILogger<JobTaskRepository> _logger;

        public JobTaskRepository(Context context, ILogger<JobTaskRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ICollection<JobTask>> GetAllAsync()
        {
            // Eager load related data for display lists
            return await _context.JobTasks
                                 .Include(j => j.Officer)
                                 .Include(j => j.Pothole)
                                 .Include(j => j.RepairTeam)
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<JobTask?> GetByIdAsync(int id)
        {
            return await _context.JobTasks
                                 .Include(j => j.Officer)
                                 .Include(j => j.Pothole)
                                 .Include(j => j.RepairTeam)
                                 .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<ICollection<JobTask>> GetJobTasksByOfficerIdAsync(int officerId)
        {
            return await _context.JobTasks
                                 .Where(j => j.OfficerId == officerId)
                                 .Include(j => j.Pothole) // Include pothole so we see what the task is
                                 .ToListAsync();
        }

        public async Task<JobTask> AddAsync(JobTask jobTask)
        {
            await _context.JobTasks.AddAsync(jobTask);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created job task {JobTaskId} for officer {OfficerId}", jobTask.Id, jobTask.OfficerId);
            return jobTask;
        }

        public async Task UpdateAsync(JobTask jobTask)
        {
            _context.JobTasks.Update(jobTask);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated job task {JobTaskId}", jobTask.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var jobTask = await _context.JobTasks.FindAsync(id);
            if (jobTask != null)
            {
                _context.JobTasks.Remove(jobTask);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted job task {JobTaskId}", id);
            }
            else
            {
                _logger.LogWarning("DeleteAsync: job task {JobTaskId} not found", id);
            }
        }
        public async Task<bool>  PotholeWithTaskExistAsync(int potholeId)
        {
            return await _context.JobTasks.AnyAsync(j => j.PotholeId == potholeId);
        }
    }
}
