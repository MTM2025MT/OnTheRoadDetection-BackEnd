using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PotholeApi.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PotholeApi.Services.RoleHandlers
{
    public class ManagerHandler : IRoleHandler
    {
        private readonly Context _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ManagerHandler(
            Context context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Define the specific role name for this handler
        public string RoleName => "Manager";

        public async Task HandleAsync(BaseRoleAssignmentDto dto)
        {
            // 1. Validate the specific DTO type
            if (dto is not ManagerAssignmentDto managerDto)
                throw new InvalidOperationException("Invalid DTO for Manager");

            // 2. Locate the Identity user
            var user = await _userManager.FindByIdAsync(managerDto.UserId.ToString());
            if (user == null)
                throw new InvalidOperationException($"User with id '{managerDto.UserId}' not found.");

            // 3. Check if user already has this role
            if (await _userManager.IsInRoleAsync(user, RoleName))
            {
                throw new Exception($"User is already in the {RoleName} role.");
            }

            // 4. Ensure the role exists in the database
            if (!await _roleManager.RoleExistsAsync(RoleName))
            {
                var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(RoleName));
                if (!createRoleResult.Succeeded)
                    throw new InvalidOperationException($"Failed to create {RoleName} role.");
            }

            // 5. Assign the user to the role
            var addResult = await _userManager.AddToRoleAsync(user, RoleName);

            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }

            // Optional: If you have a specific 'Managers' table in your Context that needs 
            // to be updated alongside the Identity Role, you would add that logic here 
            // using _context.
        }
    }
}