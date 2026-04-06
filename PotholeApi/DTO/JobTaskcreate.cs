using PotholeApi.Models;
using System.ComponentModel.DataAnnotations.Schema;
using static PotholeApi.Models.Enums;

namespace PotholeApi.DTO
{
    public class JobTaskcreate
    {



        public int PotholeId { get; set; }
        public int OfficerId { get; set; }
        public int RepairTeamId { get; set; }
        public Priority? Priority { get; set; }     // e.g. "urgent"

        public string? Notes { get; set; }       // task-specific notes
        public string? Materials { get; set; }   // materials needed/used
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Budget { get; set; }
    }
}
