
namespace PotholeApi.Models
{
    public class RepairTeam
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string ContactEmail { get; set; }
        public required string ContactPhone { get; set; }
        public required string Notes { get; set; }


        // Reverse navigation: one RepairTeam -> many Tasks
        public ICollection<JobTask> Tasks { get; set; } = new List<JobTask>();
    }
}