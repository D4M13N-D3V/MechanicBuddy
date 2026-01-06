using Carmasters.Core.Application.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace Carmasters.Core.Application.Services
{
    public interface ISmtpClientFactory
    {
        SmtpClient CreateClient();
    }
    public class SmtpClientFactory : ISmtpClientFactory
    {
        private readonly SmtpOptions smtp;
        public SmtpClientFactory(IOptions<SmtpOptions> smtp)
        {
            this.smtp = smtp.Value;
        }

        public SmtpClient CreateClient()
        {
            var client = new SmtpClient(smtp.Host, smtp.Port);
            if (!string.IsNullOrWhiteSpace(smtp.User) && !string.IsNullOrWhiteSpace(smtp.Password)) 
            {
                client.EnableSsl = true;
                client.Credentials = new System.Net.NetworkCredential(smtp.User, smtp.Password);
               
            }
            return client;
        }
    }
}
