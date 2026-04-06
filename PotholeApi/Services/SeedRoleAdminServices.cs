using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PotholeApi.IRepositories;
using PotholeApi.Models;
using System.Linq;

namespace PotholeApi.Services
{
    public class SeedRoleAdminServices : ISeedRoleAdminServices
    {
        private readonly Context _context;
        private readonly UserManager<ApplicationUser> _usermanger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOfficerRepository _officerRepository;
        public SeedRoleAdminServices( IOfficerRepository officerRepository, UserManager<ApplicationUser> UserManager, RoleManager<IdentityRole> RoleManager, Context context)
        {
            _usermanger = UserManager;
            _roleManager = RoleManager;
            _officerRepository = officerRepository;
            _context = context;
        }
        public async Task<bool> EnterNewRoleForUser(ApplicationUser user, string role)
        {

            var realUser = await _usermanger.FindByIdAsync(user.Id);

            if (realUser == null)
            {
                Console.WriteLine($"User '{user.Id}' not found.");
                return false;
            }
            if(realUser.UserName == null)
            {
                Console.WriteLine($"UserName for user '{user.Id}' is null.");
                return false;
            }
            // ✅ Prevent duplicate assignment
            if (await _usermanger.IsInRoleAsync(realUser, role))
            {
                return true; // already assigned → success
            }

            var result = await _usermanger.AddToRoleAsync(realUser, role);

            if (result.Succeeded)
            {
                return true;
            }

            foreach (var error in result.Errors)
            {
                Console.WriteLine(error.Description);
            }

            return false;
        }

        // 1. Change return type to Task<bool>
        public async Task<bool> CreateRoleAsync(string roleName)
        {
            // 2. Use 'await' instead of '.Result' (prevents deadlocks)
            IdentityResult res = await _roleManager.CreateAsync(new IdentityRole(roleName));

            // 3. Simply return the success status
            return res.Succeeded;
        }

                public async Task<IEnumerable<IdentityRole>> GetAllRolesAsync()
        {
            // Return all roles from RoleManager (materialize to list)
            return await _roleManager.Roles.ToListAsync();
        }

        // New DTO to return role + user count
        public record RoleWithCount(string RoleId, string Name, int UserCount);

        public async Task<IEnumerable<RoleWithCount>> GetRolesWithUserCountsAsync()
        {
            // Count users per role from AspNetUserRoles
            var roleCounts = await _context.Set<IdentityUserRole<string>>()
                .GroupBy(ur => ur.RoleId)
                .Select(g => new { RoleId = g.Key, Count = g.Count() })
                .ToListAsync();

            var roles = await _roleManager.Roles.ToListAsync();

            var result = roles.Select(r =>
                new RoleWithCount(
                    r.Id,
                    r.Name ?? string.Empty,
                    roleCounts.FirstOrDefault(rc => rc.RoleId == r.Id)?.Count ?? 0
                )
            );

            return result;
        }

        public async Task<bool> DeleteRoleAsync(string roleName)
        {
            var role = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role != null)
            {
                var result = await _roleManager.DeleteAsync(role);
                return result.Succeeded;
            }
            else
            {
                return false;
            }
        }

    }
}
