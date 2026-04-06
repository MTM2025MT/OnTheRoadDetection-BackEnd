using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PotholeApi.DTO;
using PotholeApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PotholeApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase

    {
        //private readonly Context _context;

        //public AccountController(Context context)
        //{
        //    _context = context;
        //}
        

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly Context _context;
        // --- NEW: Inject ISeedRoleAdminServices ---
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(Context context, UserManager<ApplicationUser> userManager, IConfiguration configuration, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _configuration = configuration;
            _roleManager = roleManager;
            _context = context;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto RegisterDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newUser = new ApplicationUser
            {
                UserName = RegisterDto.UserName,
                Email = RegisterDto.Email,
                FirstName = RegisterDto.FirstName,
                LastName = RegisterDto.LastName
            };

            IdentityResult result = await _userManager.CreateAsync(newUser, RegisterDto.Password);
            // Registration logic here
            if (result.Succeeded)
            {
                try
                {
                    // Ensure the "Officer" role exists
                    const string officerRoleName = "Officer";
                    if (!await _roleManager.RoleExistsAsync(officerRoleName))
                    {
                        var roleResult = await _roleManager.CreateAsync(new IdentityRole(officerRoleName));
                        if (!roleResult.Succeeded)
                        {
                            foreach (var err in roleResult.Errors)
                            {
                                ModelState.AddModelError("RoleCreation", err.Description);
                            }
                            return BadRequest(ModelState);
                        }
                    }

                    // Assign the new user to the "Officer" role
                    var addToRoleResult = await _userManager.AddToRoleAsync(newUser, officerRoleName);
                    if (!addToRoleResult.Succeeded)
                    {
                        foreach (var err in addToRoleResult.Errors)
                        {
                            ModelState.AddModelError("RoleAssignment", err.Description);
                        }
                        return BadRequest(ModelState);
                    }

                    // Optional: create a domain Officer record linking to the ApplicationUser
                    // Uncomment if you want to maintain an Officer entity in the DB for this user.
                    /*
                    var officer = new Officer
                    {
                        ApplicationUserId = newUser.Id,
                        Rank = null,
                        Department = null,
                        DepartmentName = null,
                        DistrictId = 0 // set appropriate default or accept from DTO
                    };
                    _context.Officers.Add(officer);
                    await _context.SaveChangesAsync();
                    */
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
      

                

                return Ok("created");
            }
            foreach (var err in result.Errors)
            {
                if (err.Code.Contains("Password"))
                {
                    ModelState.AddModelError("Password", err.Description);
                }
                else if (err.Code == "DuplicateUserName")
                {
                    ModelState.AddModelError("UserName", err.Description);
                }
                else if (err.Code == "DuplicateEmail")
                {
                    ModelState.AddModelError("Email", err.Description);
                }
                else
                {
                    // fallback: general error
                    ModelState.AddModelError("", err.Description);
                }


            }

            return BadRequest(ModelState);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApplicationUser? user = null;

            // --- YOUR ORIGINAL LOGIC: Find by Email OR UserName ---
            if (!string.IsNullOrWhiteSpace(loginDto.Email))
            {
                user = await _userManager.FindByEmailAsync(loginDto.Email);
            }
            else if (!string.IsNullOrWhiteSpace(loginDto.UserName))
            {
                user = await _userManager.FindByNameAsync(loginDto.UserName);
            }
            // -----------------------------------------------------

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized("Invalid username or password.");
            }
            var res = await _userManager.GetRolesAsync(user);
        
            var claims = new List<Claim>();
            if (string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, user.Email));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            }
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            } 
            var userRoles2 = await _userManager.GetRolesAsync(user);
            Console.WriteLine("//////////////////");
            Console.WriteLine(user);
            Console.WriteLine(user.Id);
            Console.WriteLine("//////////////////");
            Console.WriteLine($"ROLE COUNT = {userRoles.Count}");
            foreach (var r in userRoles2)
            {
                Console.WriteLine($"ROLE = {r}");
            }

            // 2. Generate Access Token (JWT)
            var secretkey = _configuration["JWT:Secret"];
            var signkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey));
            var credentials = new SigningCredentials(signkey, SecurityAlgorithms.HmacSha256);

            var myIssuer = "PotholeServer";
            var myAudience = "PotholeReactApp";

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = myIssuer,
                Audience = myAudience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15), // Access Token: Short Life (15 mins)
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            // ==========================================================
            // NEW PART: Generate and Save Refresh Token
            // ==========================================================

            // 3. Generate Refresh Token using your new Helper class
            var refreshToken = PotholeApi.Helpers.GenerateRefreshToken.GenerateTheToken();

            // 4. Update User in DB with the new Refresh Token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Refresh Token: Long Life (7 days)

            await _userManager.UpdateAsync(user);

            // 5. Set HttpOnly Cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,   // JavaScript cannot read this (Security)
                Expires = user.RefreshTokenExpiryTime,
                Secure = true,     // Must use HTTPS
                SameSite = SameSiteMode.None // Required for Cross-Site requests
            };

            Response.Cookies.Append("jwt", refreshToken, cookieOptions);

            // ==========================================================
            //Console.WriteLine(_context.Database.GetConnectionString());
            // Return the Access Token (Frontend uses this immediately)
            return Ok(new { token = accessToken, expiration = tokenDescriptor.Expires, id = user.Id ,roles= userRoles2 });
        }
        [HttpGet("GetUserById")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID '{id}' not found.");
            }
            var userDto = new
            {
                user.Id,
                user.UserName,
                user.Email
            };
            return Ok(userDto);

        }

        [HttpPut("UpdateUser")]
        public async Task<IActionResult> UpdateUser(UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(updateUserDto.Id);
            if (user == null)
            {
                return NotFound($"User with ID '{updateUserDto.Id}' not found.");
            }
            // Update user properties
            user.UserName = updateUserDto.UserName ?? user.UserName;
            user.Email = updateUserDto.Email ?? user.Email;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok("User updated successfully.");
            }
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError("", err.Description);
            }
            return BadRequest(ModelState);
        }

        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(changePasswordDto.UserId);
            if (user == null)
            {
                return NotFound($"User with ID '{changePasswordDto.UserId}' not found.");
            }
            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (result.Succeeded)
            {
                return Ok("Password changed successfully.");
            }
            foreach (var err in result.Errors)
            {
                ModelState.AddModelError("", err.Description);
            }
            return BadRequest(ModelState);
        }
        [HttpGet("Refresh-Token")]
        public async Task<IActionResult> RefreshToken()
        {
            // 1. Get the Refresh Token from the Cookie
            if (!Request.Cookies.TryGetValue("jwt", out var refreshToken))
            {
                return BadRequest("No refresh token cookie found");
            }

            // 2. Find the user who owns this specific token
            // Note: In a real app, you might want to optimize this query instead of scanning all users
            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                return Unauthorized("Invalid Refresh Token");
            }

            // 3. Check if the token has expired
            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
            {
                return Unauthorized("Refresh Token Expired");
            }

            // 4. Generate a NEW Access Token
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.NameIdentifier, user.Id)
        // Add roles again here...
    };

            var secretkey = _configuration["JWT:Secret"];
            var signkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretkey));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "PotholeServer",
                Audience = "PotholeReactApp",
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15), // New 15 min life
                SigningCredentials = new SigningCredentials(signkey, SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var newAccessToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            // (Optional) Rotate the Refresh Token here for extra security
            // var newRefreshToken = GenerateRefreshToken();
            // user.RefreshToken = newRefreshToken;
            // await _userManager.UpdateAsync(user);
            // Response.Cookies.Append("jwt", newRefreshToken, ...);

            return Ok(new { token = newAccessToken });
        }
    }

}
