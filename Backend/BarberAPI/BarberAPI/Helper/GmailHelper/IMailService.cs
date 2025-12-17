namespace BarberAPI.Helper.GmailHelper
{
    public interface IMailService
    {
        // sonradan async hale getirilebilir
        Task SendEmailAsync(SendEmailRequest sendEmailRequest);
    }
}
