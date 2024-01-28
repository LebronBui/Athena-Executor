using System.Net;
using System.Net.Mail;

namespace RealEstateAuction.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(String email, String subject, String message)
        {
            var mail = "f4landproject@gmail.com";
            var pw = "jyaqrcwfltgktweh";

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(mail, pw),
                EnableSsl = true
            };

            return client.SendMailAsync(
                               new MailMessage(
                                   from: mail,
                                   to: email,
                                   subject,
                                   message
                                   ));
        }
    }
}
