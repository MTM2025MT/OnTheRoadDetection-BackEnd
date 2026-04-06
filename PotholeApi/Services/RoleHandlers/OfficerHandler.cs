using Microsoft.AspNetCore.Identity;
using PotholeApi.Models;

namespace PotholeApi.Services.RoleHandlers
{
    public class OfficerHandler : IRoleHandler
    {
        private readonly Context _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        // Inject other services if needed

        public OfficerHandler(Context context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _context = context;
            _roleManager = roleManager;
        }

        public string RoleName => "Officer";

        public async Task HandleAsync(BaseRoleAssignmentDto dto)
        {
            // 1. Cast the generic DTO to the specific one
            if (dto is not OfficerAssignmentDto officerDto)
                throw new InvalidOperationException("Invalid DTO for Officer");
            var user = await _userManager.FindByIdAsync(officerDto.UserId.ToString());
            try
            {
                
            if(await _userManager.IsInRoleAsync(user, RoleName))
            {
                throw new Exception( "User is already in the Officer role.");
            }



            // Ensure the role exists
            if (!await _roleManager.RoleExistsAsync(RoleName))
            {
                var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(RoleName));
                if (!createRoleResult.Succeeded)
                    throw new InvalidOperationException("Failed to create Officer role.");
            }
            // Optionally persist any additional admin-specific data using _context here.
            var addResult = await _userManager.AddToRoleAsync(user, RoleName);

            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException(errors);
            }
                // 2. Run Officer-specific logic
                if (_context.Officers.Any(o => o.ApplicationUserId == officerDto.UserId))
                {
                    throw new Exception("Officer record already exists for this user.");
                }
                var officer = new Officer
                {
                    ApplicationUser = await _userManager.FindByIdAsync(officerDto.UserId),
                    BadgeNumber = officerDto.BadgeNumber, // We have access to this now!
                    Rank = officerDto.Rank,
                };

                _context.Officers.Add(officer);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
    }
}
