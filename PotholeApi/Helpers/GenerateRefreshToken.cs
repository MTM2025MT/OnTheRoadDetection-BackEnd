using System.Security.Cryptography;

namespace PotholeApi.Helpers
{
    public class GenerateRefreshToken
    {
        public  static string GenerateTheToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
