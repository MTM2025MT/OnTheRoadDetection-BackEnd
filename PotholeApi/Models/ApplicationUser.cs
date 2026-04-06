using Microsoft.AspNetCore.Identity;
namespace PotholeApi.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
