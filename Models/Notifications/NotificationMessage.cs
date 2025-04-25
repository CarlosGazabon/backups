using System;
using System.ComponentModel.DataAnnotations;

namespace Inventio.Models.Notifications
{
    public enum NotificationStatus
    {
        New,
        Opened,
        Deleted
    }
    public class NotificationMessage
    {
        public int Id { get; set; }
        public DateTime SentTime { get; set; } = DateTime.UtcNow;
        public required string Subject { get; set; }
        public required string Body { get; set; }
        public required NotificationStatus Status { get; set; }
        public required string MessageToId { get; set; }
        public virtual User? MessageTo { get; set; }

    }
}
