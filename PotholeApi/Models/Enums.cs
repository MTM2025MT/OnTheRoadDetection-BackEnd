namespace PotholeApi.Models
{
    public class Enums
    {
        public enum Level
        {
            Low=1,
            Medium,
            High,
            Critical
        }
        public enum Priority
        {
            Low=1,
            Medium,
            Urgent,
            Emergency
        }
        public enum RepairStatus
        {
            Pending = 1,
            InProgress,
            Fixed
        }
    }
}
