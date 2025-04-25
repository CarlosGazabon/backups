using System.ComponentModel.DataAnnotations;

namespace Inventio.Models.Notifications
{
    public class NotificationInventory
    {
        public int Id { get; set; }
        public required string EventName { get; set; }
        public required string Roles { get; set; }
        public int NotificationTemplateId { get; set; }
        public bool SendEmail { get; set; } = false;
        public virtual NotificationTemplate? NotificationTemplate { get; set; }
    }
}
