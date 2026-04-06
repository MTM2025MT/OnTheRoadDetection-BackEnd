using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using PotholeApi.Models;
using System.Threading.Tasks;
using static PotholeApi.Services.SeedRoleAdminServices;

namespace PotholeApi.Services
{
    public interface ISeedRoleAdminServices
    {
        Task<bool> EnterNewRoleForUser(ApplicationUser user, string role);
        Task<bool> CreateRoleAsync(string roleName);
        Task<IEnumerable<IdentityRole>> GetAllRolesAsync();
        Task<bool> DeleteRoleAsync(string roleName);
        Task<IEnumerable<RoleWithCount>> GetRolesWithUserCountsAsync();
    }
}
