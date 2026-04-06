using System.Linq;

namespace PotholeApi.Models
{
    /*
    PSEUDOCODE / PLAN:
    1. Expose a read-only property `BadgedNumber` derived from the existing `Id` property inherited
       from `IdentityRole`.
    2. If `Id` is null, empty, or whitespace -> return null (no badge can be generated).
    3. Normalize the Id:
       - Remove any non-alphanumeric characters (e.g., hyphens).
       - Convert to upper-case for consistency.
    4. Create a concise badge token:
       - Use the first N characters (N = 8) of the cleaned Id to keep the badge short.
       - If the cleaned Id is shorter than N, use the full cleaned Id.
    5. Optionally prefix the token (e.g., "B-") to make the badge clearly identifiable.
    6. Return the formatted badge string as `BadgedNumber`.
    */

    public class IdentityRoleApplication : Microsoft.AspNetCore.Identity.IdentityRole
    {
        public string? Description { get; set; }

        public string? BadgedNumber
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Id))
                    return null;

                // Remove non-alphanumeric characters and normalize to upper-case
                var cleaned = new string(Id.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

                // Take a short token (first 8 chars) for the badge
                const int tokenLength = 8;
                var token = cleaned.Length <= tokenLength ? cleaned : cleaned.Substring(0, tokenLength);

                // Prefix to indicate this is a badge number
                return $"B-{token}";
            }
        }
    }
}
