    using Microsoft.EntityFrameworkCore;
using PotholeApi.IRepositories;
using PotholeApi.Models;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
namespace PotholeApi.Repositories
{

    public class RepairTeamRepository : IRepairTeamRepository
    {
        private readonly Context _context;

        public RepairTeamRepository(Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RepairTeam>> GetAllAsync()
        {
            return await _context.RepairTeams
                                 .AsNoTracking() // Optimization for read-only lists
                                 .ToListAsync();
        }

        public async Task<RepairTeam?> GetByIdAsync(int id)
        {
            return await _context.RepairTeams.FindAsync(id);
        }

        public async Task<RepairTeam?> GetRepairTeamWithTasksAsync(int id)
        {
            // Eagerly load the 'Tasks' collection so you can see what the team is working on
            return await _context.RepairTeams
                                 .Include(rt => rt.Tasks) // Includes the related JobTasks
                                 .FirstOrDefaultAsync(rt => rt.Id == id);
        }

        public async Task AddAsync(RepairTeam repairTeam)
        {
            await _context.RepairTeams.AddAsync(repairTeam);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RepairTeam repairTeam)
        {
            _context.RepairTeams.Update(repairTeam);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var repairTeam = await _context.RepairTeams.FindAsync(id);
            if (repairTeam != null)
            {
                _context.RepairTeams.Remove(repairTeam);
                await _context.SaveChangesAsync();
            }
        }
        public async Task DoesExist(int id)
        {
            var exists = await _context.RepairTeams.AnyAsync(rt => rt.Id == id);
            if (!exists)
            {
                throw new KeyNotFoundException($"RepairTeam with ID {id} does not exist.");
            }
        }
    }
}
