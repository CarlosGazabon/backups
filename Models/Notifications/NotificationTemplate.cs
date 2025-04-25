using System.ComponentModel.DataAnnotations;

namespace Inventio.Models.Notifications
{
    public class NotificationTemplate
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
    }
}
