using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace TestRunReportRESTService
{
    class NotificationManager
    {
        internal static void SendEmail(MailMessage message, string subject)
        {
            message.Subject = subject;

            var smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(message.From.ToString(), ConfigurationManager.AppSettings["password"])
            };

            smtpClient.Send(message);
        }
    }
}
