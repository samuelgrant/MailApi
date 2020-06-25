using Mailgun.Messages;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MailApi
{
    public interface IMailService
    {
        public string RECAPTCHA_SECRET { get; set; }
        public string RECAPTCHA_KEY { get; set; }
        public string ALLOWED_SITES { set; }
        public string MAILGUN_API_KEY { get; set; }

        bool SendTo(string url, out Recipient r);
        string[] AllowedHosts();
    }

    public class MailService : IMailService
    {
        public string RECAPTCHA_SECRET { get; set; }
        public string RECAPTCHA_KEY { get; set; }
        public string ALLOWED_SITES { get; set; }          
        public string MAILGUN_API_KEY { get; set; }

        private Dictionary<string, string> Sites { 
            get
            {
                Dictionary<string, string> sites 
                    = new Dictionary<string, string>();

                string[] parts = ALLOWED_SITES.Split(";");

                foreach (string site in parts)
                {
                    string[] s = site.Split(":");
                    sites.Add(s[0], s[1]);
                }

                return sites;
            } 
        }

        public bool SendTo(string url, out Recipient r)
        {
            url = Regex.Replace(url, @"^(?:http(?:s)?://)?(?:www(?:[0-9]+)?\.)?", string.Empty, RegexOptions.IgnoreCase);
            if (Sites.ContainsKey(url)) {
                r = new Recipient
                {
                    Email = Sites[url]
                };

                return true;
            }

            return false;
        }

        public string[] AllowedHosts()
        {
            return Sites.Select(s => $"http://{s.Key}").ToArray() ?? null;
        }
    }
}
