using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TRS.IT.TRSManagers
{
    public class MailManager
    {
        public static void SendEmail(MimeMessage message)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) =>
                {
                    return true;
                };
                smtpClient.Connect("MAIL1.AEGONUSA.COM", 587, SecureSocketOptions.StartTls);
                smtpClient.Send(message);
                smtpClient.Disconnect(true);
            }
        }

        public static void SendEmail(string smtpServer, string fromAddress, string toAddress, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("", fromAddress));
            message.To.Add(new MailboxAddress("", toAddress));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = body;
            message.Body = bodyBuilder.ToMessageBody();

            using (var smtpClient = new SmtpClient())
            {
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) =>
                {
                    return true;
                };
                smtpClient.Connect(smtpServer, 587, SecureSocketOptions.StartTls);
                smtpClient.Send(message);
                smtpClient.Disconnect(true);
            }
        }
    }    
}
