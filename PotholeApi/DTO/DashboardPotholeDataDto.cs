using static PotholeApi.Models.Enums;

namespace PotholeApi.DTO
{
    public class DashboardPotholeDataDto
    {
        public List<DashboardNeighbourhoodDto> DashboardNeighbourhoods { get; set; }
        public Dictionary<Level, int> LevelCounts { get; set; } = new()
        {
            { Level.Low, 0 },
            { Level.Medium, 0 },
            { Level.High, 0 },
            { Level.Critical, 0 }
        };
        public int NumberOfPotholes => LevelCounts.Values.Sum();

    }
    public class DashboardNeighbourhoodDto
    {
       public string name { get; set; }
        public int  NumberOfpotholes { get; set; }
        public double severityOfNeighbourhood { get; set; }
        public double cost { get; set; }
    }
}
