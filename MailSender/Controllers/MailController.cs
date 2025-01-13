using Business;
using Entities;
using Microsoft.AspNetCore.Mvc;

namespace MailSender.Controllers
{
    public class MailController : Controller
    {
        private readonly IMailService mailService;
        public MailController(IMailService mailService)
        {
            this.mailService = mailService;
        }
        [HttpPost("send")]
        public async Task<IActionResult> SendMail()
        {
            try
            {
                var mailRequest = new MailRequest
                {
                    ToEmail = "mehmetsimbil@icloud.com",
                    Subject = "Test",
                    Body = "Test",
                    Attachments = new List<IFormFile>() // Eğer ek varsa, bu listeyi doldurun
                };
                await mailService.SendEmailAsync(mailRequest);

                return Ok();
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
