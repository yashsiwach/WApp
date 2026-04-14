namespace WalletService.Domain.Entities;

/// <summary>
/// Immutable record of a single CREDIT or DEBIT movement in a wallet's ledger.
/// </summary>
public class LedgerEntry
{
    /// <summary>
    /// Unique identifier of this ledger entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Identifier of the wallet this entry belongs to.
    /// </summary>
    public Guid WalletId { get; set; }
    /// <summary>
    /// Direction of the movement: CREDIT or DEBIT.
    /// </summary>
    public string Type { get; set; } = string.Empty; // CREDIT / DEBIT
    /// <summary>
    /// Monetary amount of this ledger movement.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Primary key of the source transaction record (TopUpRequest or TransferRequest).
    /// </summary>
    public Guid ReferenceId { get; set; }
    /// <summary>
    /// Category of the source transaction (e.g. TopUp, Transfer, Redemption).
    /// </summary>
    public string ReferenceType { get; set; } = string.Empty; // TopUp, Transfer, Redemption
    /// <summary>
    /// Human-readable note describing this ledger movement.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the ledger entry was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    /// <summary>
    /// Navigation property to the wallet that owns this ledger entry.
    /// </summary>
    public WalletAccount Wallet { get; set; } = null!;
}
