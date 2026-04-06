using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static PotholeApi.Models.Enums;

namespace PotholeApi.Models
{
    public class JobTask
    {
        public int Id { get; set; }

        // Pothole relation (one Task -> one Pothole)
        [Required(ErrorMessage = "Pothole is required")]
        public required int PotholeId { get; set; }
        public Pothole? Pothole { get; set; }

        // Officer relation (one Task -> one Officer)
        [Required(ErrorMessage = "Officer is required")]
        public required int OfficerId { get; set; }
        public Officer? Officer { get; set; }

        // RepairTeam relation (one Task -> one RepairTeam)
        [Required(ErrorMessage = "RepairTeam is required")]
        public required int RepairTeamId { get; set; }
        public RepairTeam? RepairTeam { get; set; }

        [Required(ErrorMessage = "AssignedAt is required")]
        public required DateTime AssignedAt { get; set; }

        // Additional nullable fields requested by the client.
        // Note: Pothole.ReportedAt already exists in the Pothole model, so we do NOT duplicate it here.

        // Make Priority non-nullable and default to Low (value 1)
        public Priority? Priority { get; set; }     // e.g. "urgent"

        public string? Notes { get; set; }       // task-specific notes
        public string? Materials { get; set; }   // materials needed/used
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Budget { get; set; }     // nullable budget amount
    }
}
