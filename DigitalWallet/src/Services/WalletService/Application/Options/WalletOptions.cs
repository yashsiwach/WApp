namespace WalletService.Application.Options;

public sealed class WalletOptions
{
    public const string SectionName = "WalletLimits";

    public decimal DailyTopUpLimit    { get; set; } = 50_000m;
    public decimal DailyTransferLimit { get; set; } = 25_000m;
    public int     MaxDailyTransfers  { get; set; } = 10;
}
