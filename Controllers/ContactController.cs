using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Models;
using System.Net;
using System.Net.Mail;

namespace Portfolio.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ContactController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult SendMessage([FromBody] ContactRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    message = "Please fill in all fields."
                });
            }

            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
                var receiverEmail = _configuration["EmailSettings:ReceiverEmail"];

                using var smtp = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(senderEmail, senderPassword)
                };

                using var mail = new MailMessage
                {
                    From = new MailAddress(senderEmail!),
                    Subject = $"Portfolio Contact: {request.Name}",
                    Body = $"""
                        You received a new message from your portfolio.

                        Name: {request.Name}
                        Email: {request.Email}

                        Message:
                        {request.Message}
                        """
                };

                mail.To.Add(receiverEmail!);
                mail.ReplyToList.Add(new MailAddress(request.Email));

                smtp.Send(mail);

                return Ok(new
                {
                    message = "Message sent successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to send message.",
                    error = ex.Message
                });
            }
        }
    }
}