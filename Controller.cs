using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using reCAPTCHA.AspNetCore;
using Mailgun.Messages;
using Mailgun.Service;
using System.Net.Http;

namespace MailApi
{
    public class Controller : ControllerBase
    {
        private IRecaptchaService Recaptcha;
        private IMailService MailSettings;

        public Controller(IRecaptchaService recaptcha, IMailService config)
        {
            Recaptcha = recaptcha;
            MailSettings = config;
        }

        [HttpGet, Route("/"), Produces("application/json")]
        public async Task<IActionResult> Index()
        {
            return Ok(new Dictionary<string, string> {
                { "ApiStatus", "Running" },
                { "Since", $"{Process.GetCurrentProcess().StartTime.ToString()}" },
                { "API Version", $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}" },
                { "reCaptchaKey", MailSettings.RECAPTCHA_KEY }
            });
        }

        [HttpPost, Route("/"), Produces("application/json")]
        public async Task<IActionResult> PostMail([FromForm] Email email)
        {
            // Validate the payload and return relevant errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(ms => ms.Value.Errors.Any()).ToList();

                Dictionary<string, string[]> errdic = new Dictionary<string, string[]>();
                foreach(var err in errors)
                {
                    errdic.Add(err.Key, err.Value.Errors.Select(s => s.ErrorMessage).ToArray());
                }

                return BadRequest(errdic);
            }

            //Validate reCaptcha response
            if (!string.IsNullOrEmpty(MailSettings.RECAPTCHA_SECRET))
            {              
                RecaptchaResponse reCaptcha = await Recaptcha.Validate(email.CaptchaCode);
                if (!reCaptcha.success) return Conflict("Failed to revalidate the reCaptcha");
            }

            MessageService mailgun = new MessageService(MailSettings.MAILGUN_API_KEY, true);
            if(!MailSettings.SendTo(Request.Headers["origin"], out Recipient recipient))
            {
                return Unauthorized("Your website is not allowed to use this service");
            }

            // Build the message
            var em = new MessageBuilder()
                .AddToRecipient(recipient)
                .SetFromAddress(new Recipient
                {
                    DisplayName = email.From.Name,
                    Email = email.From.Email
                })
                .SetReplyToAddress(new Recipient
                {
                    DisplayName = email.From.Name,
                    Email = email.From.Email
                })
                .SetSubject(email.Subject)
                .SetTextBody(email.Message);

            var res = await mailgun.SendMessageAsync("mailservice.samuelgrant.dev", em.GetMessage());
            if (res.IsSuccessStatusCode)
            {
                return Ok("Thanks! We received your message.");
            }

            return BadRequest("We were unable to send your email, please try again later");
        }
    }
}
