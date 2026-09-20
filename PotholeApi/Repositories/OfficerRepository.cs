using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PotholeApi.IRepositories;
using PotholeApi.Models;
using System;

namespace PotholeApi.Repositories
{
    public class OfficerRepository : IOfficerRepository
    {
        private readonly Context _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OfficerRepository> _logger;

        public OfficerRepository(Context context, UserManager<ApplicationUser> userManager, ILogger<OfficerRepository> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ICollection<Officer>> GetAllAsync()
        {
            // Uses AsNoTracking for read-only operations to improve performance
            return await _context.Officers
                                 .AsNoTracking()
                                 .ToListAsync();
        }

        public async Task<Officer?> GetByIdAsync(int id)
        {

            return await _context.Officers.FindAsync(id);
        }

        public async Task<Officer?> GetOfficerWithJobTasksAsync(int id)
        {
            // Includes the list of JobTasks when fetching the Officer
            return await _context.Officers
                                 .Include(o => o.JobTasks)
                                 .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task AddAsync(Officer officer)
        {
            await _context.Officers.AddAsync(officer);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created officer {OfficerId}", officer.Id);
        }

        public async Task UpdateAsync(Officer officer)
        {
            _context.Officers.Update(officer);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated officer {OfficerId}", officer.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var officer = await _context.Officers.FindAsync(id);
            if (officer != null)
            {
                _context.Officers.Remove(officer);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted officer {OfficerId}", id);
            }
            else
            {
                _logger.LogWarning("DeleteAsync: officer {OfficerId} not found", id);
            }
        }
        public async Task DoesExist(int id)
        {
            var exists = await _context.Officers.AnyAsync(o => o.Id == id);
            if (!exists)
            {
                _logger.LogWarning("DoesExist: officer {OfficerId} does not exist", id);
                throw new KeyNotFoundException($"Officer with ID {id} does not exist.");
            }
        }
        public async Task<Officer> GetOfficerByUserIdAsync(string userId)
        {
            var officer = await _context.Officers
                                        .FirstOrDefaultAsync(o => o.ApplicationUser.Id == userId);
            if (officer == null)
            {
                _logger.LogWarning("GetOfficerByUserIdAsync: no officer for user {UserId}", userId);
                throw new KeyNotFoundException($"Officer with User ID {userId} does not exist.");
            }
            return officer;
        }
    }
}
