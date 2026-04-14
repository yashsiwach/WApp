namespace WalletService.Application.Options;

/// <summary>
/// Strongly-typed configuration options for wallet transaction limits.
/// </summary>
public sealed class WalletOptions
{
    public const string SectionName = "WalletLimits";

    /// <summary>
    /// Maximum total amount a user may top up in a single calendar day.
    /// </summary>
    public decimal DailyTopUpLimit    { get; set; } = 50_000m;
    /// <summary>
    /// Maximum total amount a user may transfer out in a single calendar day.
    /// </summary>
    public decimal DailyTransferLimit { get; set; } = 25_000m;
    /// <summary>
    /// Maximum number of outbound transfers allowed per user per calendar day.
    /// </summary>
    public int     MaxDailyTransfers  { get; set; } = 10;
}
