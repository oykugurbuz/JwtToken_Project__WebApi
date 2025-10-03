using System.Net;
using System.Net.Mail;

namespace JwtTokenProject.Services
{
    public interface INotificationServices
    {
        Task SendWelcomeEmailAsync(string toEmail, string userName);
    }
    public class NotificationServices : INotificationServices
    {
        private readonly IConfiguration _config;

        public NotificationServices(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com") // host özelliğini ctor içerisinde alırız
            {
                Port = 587,
                Credentials = new NetworkCredential("mailadresin", "şifren"),// google çift doğrulama ve uygulama şifresi ile giriş yap
                EnableSsl = true,
            };
            var mail = new MailMessage("mailadresin", toEmail) //mailMessage sınıfı mail nesnesi
            {
                Subject = "Hoşgeldiniz", //mail konusu
                Body = $"Merhaba {userName} kaydınız başarıyla oluşturuldu.", //mail içeriği
                IsBodyHtml = true, //mail içeriği html mi düz metin mi
            };

            await smtpClient.SendMailAsync(mail);
        }
    }
}

