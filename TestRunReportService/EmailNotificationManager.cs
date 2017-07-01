
namespace TestRunReportService
{
    using System.Configuration;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;

    using Microsoft.TeamFoundation.Build.Client;

    class EmailNotificationManager
    {
        internal static void SendEmail(string subject, IBuildDefinition buildDefinition)
        {
            var sender = ConfigurationManager.AppSettings["Sender"];
            var recipient = ConfigurationManager.AppSettings["Recipient"];

            var message = new MailMessage();

            var testRuns = BuildManager.GetTestRuns(buildDefinition);

            if (!testRuns.Any())
            {
                return;
            }

            foreach (var tr in testRuns)
            {
                message = MessageManager.GetFormattedMessage(recipient, sender, buildDefinition, tr);
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
}
