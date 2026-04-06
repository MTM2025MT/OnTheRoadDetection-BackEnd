using PotholeApi.Models;

namespace PotholeApi.DTO
{
    public class ScanExecutionTaskDto
    {
        public string VehicleId { get; set; }
        public DateTime StartTime { get; set; }
        public int EstimatedMinutes { get; set; }
        public ScanType Type { get; set; }
        public int? IntervalHours { get; set; }
        public DateTime? EndTime { get; set; }
        public ScanPriority Priority { get; set; }
        public bool AutoRescan { get; set; }
        public bool SendNotifications { get; set; }
        public string Notes { get; set; }
        public List<RoutePointDto> RoutePoints { get; set; }
        public List<GpsCoordinateDto> RoutePath { get; set; }
    }

    public class RoutePointDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Order { get; set; }
    }

    public class GpsCoordinateDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

}
