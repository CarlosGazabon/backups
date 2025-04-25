using Inventio.Data;
using Inventio.Models.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventio2Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<NotificationsController> _logger;


        public NotificationsController(ILogger<NotificationsController> logger, ApplicationDBContext context)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("notification-messages")]
        public async Task<ActionResult> GetNotificationMessages([FromQuery] string userId)
        {

            var result = await _context.NotificationMessage
                .Where(m => m.MessageToId == userId && (m.Status == NotificationStatus.New || m.Status == NotificationStatus.Opened))
                .OrderByDescending(m => m.SentTime)
                .ToListAsync();
            return Ok(result);
        }

        public class MessagesStatusCountDTO
        {
            public int NewMessagesCount { get; set; }
            public int ReadMessagesCount { get; set; }
            public int DeletedMessagesCount { get; set; }
        }

        [HttpGet("messages-status-count")]
        public async Task<ActionResult<MessagesStatusCountDTO>> GetMessagesStatusCount([FromQuery] string userId)
        {
            var newCount = await _context.NotificationMessage.CountAsync(m => m.Status == NotificationStatus.New && m.MessageToId == userId);
            var readCount = await _context.NotificationMessage.CountAsync(m => m.Status == NotificationStatus.Opened && m.MessageToId == userId);
            var deletedCount = await _context.NotificationMessage.CountAsync(m => m.Status == NotificationStatus.Deleted && m.MessageToId == userId);

            var result = new MessagesStatusCountDTO
            {
                NewMessagesCount = newCount,
                ReadMessagesCount = readCount,
                DeletedMessagesCount = deletedCount
            };

            return Ok(result);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.NotificationMessage.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            message.Status = NotificationStatus.Deleted;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/mark-as-read")]
        public async Task<IActionResult> Toggle(int id)
        {
            var message = await _context.NotificationMessage.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }

            message.Status = NotificationStatus.Opened;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
