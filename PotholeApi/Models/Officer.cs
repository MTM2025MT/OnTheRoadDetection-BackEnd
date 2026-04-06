using System.ComponentModel.DataAnnotations.Schema;

namespace PotholeApi.Models
{
    public class Officer
    {
        public int Id { get; set; }

        // Link to Identity user (string key)
        public string? ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public required string BadgeNumber { get; set; }
        public string? Rank { get; set; }
        public string? Department { get; set; }
        public string? DepartmentName { get; set; }
        public int DistrictId { get; set; }

        // This attribute tells EF that DistrictId is the key for the District object
        [ForeignKey("DistrictId")]
        public District District { get; set; }
        public ICollection<JobTask> JobTasks { get; set; } = new List<JobTask>();
    }
}
