namespace NotificationService.Application.Options;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host      { get; set; } = "smtp.gmail.com";
    public int    Port      { get; set; } = 587;
    public string Username  { get; set; } = string.Empty;
    public string Password  { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@digitalwallet.com";
    public string FromName  { get; set; } = "DigitalWallet";
    /// <summary>Set false only for local dev SMTP servers that don't support TLS.</summary>
    public bool   EnableSsl { get; set; } = true;
}
