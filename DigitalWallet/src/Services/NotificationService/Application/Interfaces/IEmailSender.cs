namespace NotificationService.Application.Interfaces;

/// <summary>Abstraction for sending transactional emails. Swap implementation without touching services.</summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlBody);
}
