namespace PotholeApi.Models
{
    public class ScanExecutionTask
    {
        // Required fields
        public  int Id { get; set; }
        public string VehicleId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int EstimatedMinutes { get; set; } = 30;

        // Execution type
        public ScanType Type { get; set; } = ScanType.OneTime;

        // For recurring scans
        public int? IntervalHours { get; set; } // null = one-time
        public DateTime? EndTime { get; set; }

        // Settings
        public ScanPriority Priority { get; set; } = ScanPriority.Normal;
        public bool AutoRescan { get; set; }
        public bool SendNotifications { get; set; } = true;

        // Optional
        public string Notes { get; set; } = string.Empty;
        public List<RoutePoint> RoutePoints { get; set; } = new List<RoutePoint>();
        public List<GpsCoordinate> RoutePath { get; set; } = new List<GpsCoordinate>();
    }
    public class RoutePoint
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Order { get; set; }

    }

    public class GpsCoordinate
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

    }
    public enum ScanType
    {
        OneTime = 0,
        Recurring = 1
    }

    public enum ScanPriority
    {
        Low = 0,
        Normal = 1,
        High = 2
    }
}