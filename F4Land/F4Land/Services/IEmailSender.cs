namespace RealEstateAuction.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(String email, String subject, String message);
    }
}
