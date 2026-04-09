using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Options;

namespace NotificationService.Infrastructure.Email;

/// <summary>Sends real emails via SMTP using MailKit. Configure credentials in appsettings.json → Smtp section.</summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger  = logger;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var fromEmail = string.IsNullOrWhiteSpace(_options.FromEmail) ? _options.Username : _options.FromEmail;
        if (_options.Host.Contains("gmail", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(_options.Username))
        {
            fromEmail = _options.Username;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, fromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body    = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        try
        {
            var secureSocketOptions = _options.EnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(_options.Host, _options.Port, secureSocketOptions);

            if (!string.IsNullOrWhiteSpace(_options.Username))
                await client.AuthenticateAsync(_options.Username, _options.Password);

            await client.SendAsync(message);
            _logger.LogInformation("Email sent to {ToEmail}: {Subject}", toEmail, subject);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
