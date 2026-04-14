namespace WalletService.Domain.Entities;

/// <summary>
/// Represents a request to add funds to a wallet via an external payment provider.
/// </summary>
public class TopUpRequest
{
    /// <summary>
    /// Unique identifier of this top-up request.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Identifier of the wallet receiving the funds.
    /// </summary>
    public Guid WalletId { get; set; }
    /// <summary>
    /// Amount to be credited to the wallet.
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// Name of the external payment provider processing the top-up.
    /// </summary>
    public string Provider { get; set; } = string.Empty;
    /// <summary>
    /// Client-supplied key used to detect and reject duplicate requests.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;
    /// <summary>
    /// Current processing status: Pending, Completed, or Failed.
    /// </summary>
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed
    /// <summary>
    /// UTC timestamp when the top-up request was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp of the last status update.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    /// <summary>
    /// Navigation property to the wallet receiving the top-up.
    /// </summary>
    public WalletAccount Wallet { get; set; } = null!;
}
