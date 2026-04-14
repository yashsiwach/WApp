namespace WalletService.Domain.Entities;

/// <summary>
/// Represents a peer-to-peer fund transfer between two wallet accounts.
/// </summary>
public class TransferRequest
{
    /// <summary>
    /// Unique identifier of this transfer request.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Identifier of the wallet from which funds are debited.
    /// </summary>
    public Guid FromWalletId { get; set; }
    /// <summary>
    /// Identifier of the wallet to which funds are credited.
    /// </summary>
    public Guid ToWalletId { get; set; }
    /// <summary>
    /// Amount to be moved between the two wallets.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Client-supplied key used to detect and reject duplicate requests.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;
    /// <summary>
    /// Current processing status: Pending, Completed, or Failed.
    /// </summary>
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    /// <summary>
    /// UTC timestamp when the transfer request was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp of the last status update.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
