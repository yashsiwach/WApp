namespace WalletService.Domain.Entities;

/// <summary>
/// Tracks cumulative top-up and transfer amounts for a wallet on a single calendar day.
/// </summary>
public class DailyLimitTracker
{
    /// <summary>
    /// Unique identifier of this daily limit record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Identifier of the wallet this tracker belongs to.
    /// </summary>
    public Guid WalletId { get; set; }
    /// <summary>
    /// The UTC calendar date this tracker covers.
    /// </summary>
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    /// <summary>
    /// Running total of top-up amounts processed today.
    /// </summary>
    public decimal TopUpTotal { get; set; } = 0m;
    /// <summary>
    /// Running total of transfer amounts sent today.
    /// </summary>
    public decimal TransferTotal { get; set; } = 0m;
    /// <summary>
    /// Number of outbound transfers completed today.
    /// </summary>
    public int TransferCount { get; set; } = 0;
    /// <summary>
    /// UTC timestamp of the last update to this record.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
