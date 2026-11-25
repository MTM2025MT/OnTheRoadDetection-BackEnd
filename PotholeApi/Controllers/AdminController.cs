using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PotholeApi.DTO;
using PotholeApi.Models;
using PotholeApi.Services;
using System.Threading.Tasks;

namespace PotholeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        private readonly ISeedRoleAdminServices _seedRoleAdminServices;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Context _context;
        private readonly IEnumerable<IRoleHandler> _roleHandlers;
        private readonly ILogger<AdminController> _logger;
        public AdminController(IEnumerable<IRoleHandler> roleHandlers, Context context, ISeedRoleAdminServices seedRoleAdminServices, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<AdminController> logger)
        {
            _seedRoleAdminServices = seedRoleAdminServices;
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _roleHandlers = roleHandlers;
            _logger = logger;
        }
        [HttpPost("CreateRole")]
        public async Task<IActionResult> CreateRole([FromBody] RoleCreateDto roleDto)
        {
            // 1. Validate Input
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 2. Async Check: Use the built-in method instead of .Any()
            // It is cleaner and safer for the database connection.
            bool roleExists = await _roleManager.RoleExistsAsync(roleDto.RoleName);

            if (roleExists)
            {
                _logger.LogWarning("CreateRole: role {RoleName} already exists", roleDto.RoleName);
                return BadRequest($"Role '{roleDto.RoleName}' already exists.");
            }


            // 3. Async Create: Use await here
            var result = await _seedRoleAdminServices.CreateRoleAsync(roleDto.RoleName);

            var isThereHandlerForThisRole = _roleHandlers.Any(h => h.RoleName == roleDto.RoleName);
            if (isThereHandlerForThisRole) {
                _logger.LogWarning("CreateRole: role {RoleName} has a dedicated handler, refusing generic creation", roleDto.RoleName);
                return BadRequest($"Role '{roleDto.RoleName}' cannot be created because it has a specific handler. Please use the appropriate endpoint for that role.");
            }
            // Optional: Check if the service actually succeeded
            // (Assuming your service returns a boolean or IdentityResult)
            // if (!result.Succeeded) return BadRequest("Could not create role.");

            _logger.LogInformation("Created role {RoleName}", roleDto.RoleName);
            return Ok($"Role '{roleDto.RoleName}' created successfully.");
        }
        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetAllRoles()
        {

            var roles = await  _seedRoleAdminServices.GetRolesWithUserCountsAsync();
            if (roles == null || !roles.Any())
            {
                return NotFound("No roles found.");
            }

            return Ok(roles);
        }
        [HttpDelete("DeleteRole")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var result = await _seedRoleAdminServices.DeleteRoleAsync(roleName);
            if (!result)
            {
                _logger.LogWarning("DeleteRole: role {RoleName} not found", roleName);
                return NotFound($"Role '{roleName}' not found.");
            }
            _logger.LogInformation("Deleted role {RoleName}", roleName);
            return Ok($"Role '{roleName}' deleted successfully.");
        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] BaseRoleAssignmentDto dto)
        {
            // 1. Determine the Role Name from the DTO type (or the discriminator)
            // Since we configured Polymorphism, 'dto' is ALREADY the specific type (e.g., OfficerAssignmentDto)


            string targetRole = dto.RoleName;

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // 2. Assign the Identity Role (Common for everyone)
                var user = await _userManager.FindByIdAsync(dto.UserId);
                //bool success = await _seedRoleAdminServices.EnterNewRoleForUser(user, targetRole);
                //if (!success) return BadRequest("Could not assign Identity role.");

                // 3. Find the specific Handler for this role
                var handler = _roleHandlers.FirstOrDefault(h => h.RoleName == targetRole);

                if (handler != null)
                {
                    // 4. Run the specific logic (Create Officer, etc.)
                    await handler.HandleAsync(dto);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Assigned role {RoleName} to user {UserId}", targetRole, dto.UserId);
                return Ok($"{targetRole} assigned and profile created.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "AssignRole: failed to assign role {RoleName} to user {UserId}", targetRole, dto.UserId);
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAllUser()
        {
            var users = _userManager.Users.ToList();
            var usersDto = new List<object>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                usersDto.Add(new
                {
                    u.FirstName,
                    u.LastName,
                    u.Id,
                    u.UserName,
                    u.Email,
                    Roles = roles
                });
            }

            return Ok(usersDto);
        }


    }   
}
