using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using reCAPTCHA.AspNetCore;
using Mailgun.Messages;
using Mailgun.Service;

namespace MailApi
{
    public class Controller : ControllerBase
    {
        private IConfiguration Configuration;
        //private IRecaptchaService Recaptcha;

        public Controller(IConfiguration config)//, IRecaptchaService recaptcha
        {
            Configuration = config;
            //Recaptcha = recaptcha;
        }

        [HttpGet, Route("/"), Produces("application/json")]
        public async Task<IActionResult> Index()
        {
            return Ok(new Dictionary<string, string> {
                { "ApiStatus", "Running" },
                { "Since", $"{Process.GetCurrentProcess().StartTime.ToString()}" },
                { "API Version", $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}" },
                { "reCaptchaKey", Configuration["RECAPTCHA_KEY"] }
            });
        }

        [HttpPost, Route("/"), Produces("application/json")]
        public async Task<IActionResult> SubmitMail([FromBody] Email email)
        {
            // Validate the payload and return relevant errors
            if (!ModelState.IsValid)
            {
                IEnumerable<string> errors = ModelState.Values.SelectMany(v => v.Errors.Select(s => s.ErrorMessage));
                return BadRequest(errors);
            }

            // Validate reCaptcha response
            //string recaptchaSecreat = Configuration["RECAPTCHA_SECREAT"];
            //if (!string.IsNullOrEmpty(recaptchaSecreat))
            //{
            //    RecaptchaResponse reCaptcha = await Recaptcha.Validate(email.CaptchaCode);
            //    if (!reCaptcha.success) return Conflict("Failed to revalidate the reCaptcha");
            //}

            MessageService mailgun = new MessageService(Configuration["MAILGUN_API_KEY"]);
            // Build the message
            var em = new MessageBuilder()
                .AddToRecipient(new Recipient {
                    DisplayName = email.From.Name,
                    Email = email.From.Email
                })
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

            var res = await mailgun.SendMessageAsync("workingDomain", em.GetMessage());
            if (!res.IsSuccessStatusCode)
            {
                return Ok("Thanks! We received your message.");
            }

            return BadRequest("We were unable to send your email, please try again later");
        }
    }
}
