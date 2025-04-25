using inventio.Models;
using inventio.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace inventio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailSender _emailSender;

        public EmailController(IEmailSender emailSender)
        {
            this._emailSender = emailSender;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult HelloWorld()
        {
            return Ok("Hello World");
        }

        [AllowAnonymous]
        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailSchema request)
        {
            try
            {
                await _emailSender.SendEmailAsync(request);
                return Ok(new { message = "Email sent successfully" });
            }
            catch (System.Exception ex)
            {
                // Handle any exception that might occur while sending the email
                return BadRequest(new { message = "Failed to send email", error = ex.Message });
            }
        }
    }
}