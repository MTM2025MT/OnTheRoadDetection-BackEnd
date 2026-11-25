
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PotholeApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PotholeApi.Services.RoleHandlers
{
    public class AdminHandler : IRoleHandler
    {
        private readonly Context _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminHandler(
            Context context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public string RoleName => "Admin";

        public async Task HandleAsync(BaseRoleAssignmentDto dto)
        {
            if (dto is not AdminAssignmentDto adminDto)
                throw new InvalidOperationException("Invalid DTO for Admin");

            var user = await _userManager.FindByIdAsync(adminDto.UserId);
            if (user == null)
                throw new InvalidOperationException($"User with id '{adminDto.UserId}' not found.");

            if (await _userManager.IsInRoleAsync(user, RoleName))
            {
                throw new InvalidOperationException("User is already in the Admin role.");
            }

            // Ensure the role exists
            if (!await _roleManager.RoleExistsAsync(RoleName))
            {
                var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(RoleName));
                if (!createRoleResult.Succeeded)
                    throw new InvalidOperationException("Failed to create Admin role.");
            }
            // Optionally persist any additional admin-specific data using _context here.
            var addResult = await _userManager.AddToRoleAsync(user, RoleName);

            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }

        }
    }
}