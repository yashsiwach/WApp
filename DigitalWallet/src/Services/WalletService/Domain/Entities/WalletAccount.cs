namespace WalletService.Domain.Entities;

/// <summary>
/// Core aggregate representing a user's digital wallet and its current state.
/// </summary>
public class WalletAccount
{
    /// <summary>
    /// Unique identifier of the wallet account.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Identifier of the user who owns this wallet.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Email address of the wallet owner, used for peer-to-peer transfer lookup.
    /// </summary>
    public string? Email { get; set; }
    /// <summary>
    /// Denormalized running balance maintained for fast read access.
    /// </summary>
    public decimal SnapshotBalance { get; set; } = 0m;
    /// <summary>
    /// ISO currency code for this wallet (default: INR).
    /// </summary>
    public string Currency { get; set; } = "INR";
    /// <summary>
    /// When true, all transactions on this wallet are blocked.
    /// </summary>
    public bool IsLocked { get; set; } = false;
    /// <summary>
    /// Indicates whether the user has completed KYC verification, enabling transfers.
    /// </summary>
    public bool KYCVerified { get; set; } = false;
    /// <summary>
    /// UTC timestamp when the wallet was first created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp of the last modification to this wallet record.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    /// <summary>
    /// All ledger entries (credits and debits) recorded against this wallet.
    /// </summary>
    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    /// <summary>
    /// All top-up requests made against this wallet.
    /// </summary>
    public ICollection<TopUpRequest> TopUpRequests { get; set; } = new List<TopUpRequest>();
}
