using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

// 1. The Base Class
// This configuration tells .NET how to pick the right subclass based on the "roleType" field in JSON
[JsonPolymorphic(TypeDiscriminatorPropertyName = "roleType")]
[JsonDerivedType(typeof(OfficerAssignmentDto), typeDiscriminator: "Officer")]
[JsonDerivedType(typeof(RepairTeamAssignmentDto), typeDiscriminator: "RepairTeam")]
[JsonDerivedType(typeof(AdminAssignmentDto), typeDiscriminator: "Admin")]
[JsonDerivedType(typeof(ManagerAssignmentDto),typeDiscriminator:"Manager")]
public abstract class BaseRoleAssignmentDto
{
    [Required]
    public string  UserId { get; set; }
    public abstract string RoleName { get; }
}

// 2. Specific DTO for Officer (Has Badge)
public class OfficerAssignmentDto : BaseRoleAssignmentDto
{
    [Required]
    public required string BadgeNumber { get; set; }
    public  string Rank { get; set; }
    public override string RoleName => "Officer";
}

// 3. Specific DTO for RepairTeam (Has Department)
public class RepairTeamAssignmentDto : BaseRoleAssignmentDto
{
    [Required]
    public string? DepartmentName { get; set; }
    public override string RoleName => "RepairTeam";
}

// 4. Admin (Maybe no extra data needed)
public class AdminAssignmentDto : BaseRoleAssignmentDto {
    public override string RoleName => "Admin";
}
public class ManagerAssignmentDto : BaseRoleAssignmentDto
{
    public override string RoleName => "Manager";

}