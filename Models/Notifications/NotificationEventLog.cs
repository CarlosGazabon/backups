namespace Inventio.Models.Notifications
{

    public enum NotificationEventLogType
    {
        NonConformanceNewIncident,
        NonConformanceEditIncident,
        NonConformanceReleaseIncident,
    }
    public class NotificationEventLog
    {
        public int Id { get; set; }

        public DateTime Time { get; set; } = DateTime.UtcNow;

        public required NotificationEventLogType Type { get; set; }

        public required string Data { get; set; }
    }
}
