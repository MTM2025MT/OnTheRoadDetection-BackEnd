namespace PotholeApi.DTO
{
    public class UpdateUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}
