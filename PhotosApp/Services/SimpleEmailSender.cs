using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PhotosApp.Services
{
    // NOTE: чтобы использовать аккаунт с gmail.com для отправки писем
    // необходимо разрешить "Небезопасные приложения" https://myaccount.google.com/lesssecureapps
    public class SimpleEmailSender : IEmailSender
    {
        private readonly ILogger<SimpleEmailSender> logger;
        private readonly IWebHostEnvironment env;
        private readonly string host;
        private readonly int port;
        private readonly bool enableSSL;
        private readonly string userName;
        private readonly string password;

        public SimpleEmailSender(ILogger<SimpleEmailSender> logger,
            IWebHostEnvironment hostingEnvironment,
            string host, int port, bool enableSSL,
            string userName, string password)
        {
            this.logger = logger;
            this.env = hostingEnvironment;
            this.host = host;
            this.port = port;
            this.enableSSL = enableSSL;
            this.userName = userName;
            this.password = password;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (env.IsDevelopment())
            {
                var message = new StringBuilder();
                message.AppendLine();
                message.AppendLine(">>> -------------------- <<<");
                message.AppendLine($"From: {userName}");
                message.AppendLine($"To: {email}");
                message.AppendLine($"Subject: {subject}");
                message.AppendLine();
                // NOTE: Можно использовать System.Web.HttpUtility.HtmlDecode,
                // чтобы URL ссылки отображался в логе как текст, а не был закодирован в HTML-сущности.
                message.AppendLine(htmlMessage);
                message.AppendLine(">>> -------------------- <<<");
                message.AppendLine();
                logger.LogInformation(message.ToString());
            }

            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(userName, password),
                    EnableSsl = enableSSL
                };
                await client.SendMailAsync(
                    new MailMessage(userName, email, subject, htmlMessage)
                    {
                        IsBodyHtml = true
                    }
                );
            }
        }
    }
}
