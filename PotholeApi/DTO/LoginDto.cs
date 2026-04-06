using System.ComponentModel.DataAnnotations;
namespace PotholeApi.DTO
{
    public class LoginDto : IValidatableObject
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }

        [Required]
        public required string Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Email) &&
                string.IsNullOrWhiteSpace(UserName))
            {
                yield return new ValidationResult(
                    "Email or UserName must be provided.",
                    new[] { nameof(Email), nameof(UserName) }
                );
            }
        }
    }
}
