using System.ComponentModel.DataAnnotations;

namespace MailApi
{
    public class Email
    {
        public From From { get; set; }
        public string CaptchaCode { get; set; }
        [Required]
        public string Message { get; set; }
        [Required]
        public string Subject { get; set; }
    }

    public class From
    {
        [Required]
        public string Name { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
