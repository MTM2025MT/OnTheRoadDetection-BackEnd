namespace PotholeApi.Services
{
    public interface IRoleHandler
    {
        // A simplified name to match the DTO discriminator (e.g., "Officer")
        string RoleName { get; }

        // The logic to save the data
        Task HandleAsync(BaseRoleAssignmentDto dto);
    }
}
