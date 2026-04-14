namespace NotificationService.Application.Options;

/// <summary>
/// Strongly-typed configuration options bound from the "Smtp" section of appsettings.json.
/// </summary>
public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    /// <summary>
    /// Hostname or IP address of the SMTP server (e.g. smtp.gmail.com).
    /// </summary>
    public string Host      { get; set; } = "smtp.gmail.com";
    /// <summary>
    /// TCP port used to connect to the SMTP server (typically 587 for STARTTLS).
    /// </summary>
    public int    Port      { get; set; } = 587;
    /// <summary>
    /// SMTP authentication username, usually the sender email address.
    /// </summary>
    public string Username  { get; set; } = string.Empty;
    /// <summary>
    /// SMTP authentication password or app-specific password.
    /// </summary>
    public string Password  { get; set; } = string.Empty;
    /// <summary>
    /// Email address shown in the From header; falls back to Username when empty.
    /// </summary>
    public string FromEmail { get; set; } = "noreply@digitalwallet.com";
    /// <summary>
    /// Display name shown in the From header of outgoing emails.
    /// </summary>
    public string FromName  { get; set; } = "DigitalWallet";
    /// <summary>Set false only for local dev SMTP servers that don't support TLS.</summary>
    public bool   EnableSsl { get; set; } = true;
}
