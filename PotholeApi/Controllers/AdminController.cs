using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public AdminController(IEnumerable<IRoleHandler> roleHandlers, Context context,ISeedRoleAdminServices seedRoleAdminServices,RoleManager<IdentityRole> RoleManager,UserManager<ApplicationUser> UserManager)
        {
            _seedRoleAdminServices = seedRoleAdminServices;
            _roleManager = RoleManager;
            _userManager = UserManager;
            _context = context;
            _roleHandlers = roleHandlers;
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
                return BadRequest($"Role '{roleDto.RoleName}' already exists.");
            }

            
            // 3. Async Create: Use await here
            var result = await _seedRoleAdminServices.CreateRoleAsync(roleDto.RoleName);

            var isThereHandlerForThisRole = _roleHandlers.Any(h => h.RoleName == roleDto.RoleName);
            if (isThereHandlerForThisRole) {
                return BadRequest($"Role '{roleDto.RoleName}' cannot be created because it has a specific handler. Please use the appropriate endpoint for that role.");
            }
            // Optional: Check if the service actually succeeded
            // (Assuming your service returns a boolean or IdentityResult)
            // if (!result.Succeeded) return BadRequest("Could not create role.");

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
        public IActionResult DeleteRole(string roleName)
        {
            var result =  _seedRoleAdminServices.DeleteRoleAsync(roleName).Result;
            if (!result)
            {
                return NotFound($"Role '{roleName}' not found.");
            }
            return Ok($"Role '{roleName}' deleted successfully.");
        }
        //[HttpPost("AssignRoleToUser")]
        //public async Task<IActionResult> AssignRoleToUserAndCreateItsClassAsync(string userId, string roleName)
        //{
        //    // Implementation for assigning role to user
        //    using var transaction = _context.Database.BeginTransaction();

        //    try
        //    {

        //        var res = _seedRoleAdminServices.EnterNewRoleForUser(new PotholeApi.Models.ApplicationUser { Id = userId }, new IdentityRole { Name = roleName }).Result;
        //        if (!res)
        //        {
        //            return BadRequest("Could not assign role to user.");
        //        }
        //        if (roleName == "Officer")
        //        {
        //            var officerCreation = _seedRoleAdminServices.CreateOfficerForUserAsync(userId).Result;
        //            if (!officerCreation)
        //            {
        //                return BadRequest("Could not create Officer profile for user.");
        //            }
        //            return Ok();
        //        }
        //        if (roleName == "RepairTeam")
        //        {
        //            var repairTeamCreation = _seedRoleAdminServices.CreateRepairTeamForUserAsync(userId).Result;
        //            if (!repairTeamCreation)
        //            {
        //                return BadRequest("Could not create RepairTeam profile for user.");
        //            }
        //            return Ok();
        //        }
        //        if (roleName == "Admin")
        //        {
        //            var adminCreation = _seedRoleAdminServices.CreateAdminForUserAsync(userId).Result;
        //            if (!adminCreation)
        //            {
        //                return BadRequest("Could not create Admin profile for user.");
        //            }
        //            return Ok();
        //        }
        //        if (roleName == "Worker")
        //        {
        //            var workerCreation = _seedRoleAdminServices.CreateWorkerForUserAsync(userId).Result;
        //            if (!workerCreation)
        //            {
        //                return BadRequest("Could not create Worker profile for user.");
        //            }
        //            return Ok();
        //        }
        //    }

        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();

        //        return BadRequest("Operation failed. Changes were undone. Error: " + ex.Message);
        //    }
        //    return Ok();
        //}

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
                var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
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
                return Ok($"{targetRole} assigned and profile created.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
