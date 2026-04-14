namespace WalletService.Application.DTOs;

// —— Balance ——
/// <summary>
/// Response DTO containing the current balance and status of a wallet.
/// </summary>
public record BalanceResponse
{
    /// <summary>
    /// Unique identifier of the wallet.
    /// </summary>
    public Guid WalletId { get; init; }
    /// <summary>
    /// Current snapshot balance of the wallet.
    /// </summary>
    public decimal Balance { get; init; }
    /// <summary>
    /// Currency code for the wallet balance (default: INR).
    /// </summary>
    public string Currency { get; init; } = "INR";
    /// <summary>
    /// Indicates whether the wallet is locked and cannot process transactions.
    /// </summary>
    public bool IsLocked { get; init; }
    /// <summary>
    /// Indicates whether the user has completed KYC verification.
    /// </summary>
    public bool KYCVerified { get; init; }
}

// —— TopUp ——
/// <summary>
/// Request DTO for adding funds to a wallet via a payment provider.
/// </summary>
public record TopUpRequestDto
{
    /// <summary>
    /// Amount to add to the wallet.
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// Name of the payment provider processing the top-up.
    /// </summary>
    public string Provider { get; init; } = string.Empty;
    /// <summary>
    /// Client-generated key to ensure idempotent processing.
    /// </summary>
    public string IdempotencyKey { get; init; } = string.Empty;
}

/// <summary>
/// Response DTO returned after a successful top-up operation.
/// </summary>
public record TopUpResponseDto
{
    /// <summary>
    /// Unique identifier of the top-up transaction.
    /// </summary>
    public Guid TransactionId { get; init; }
    /// <summary>
    /// Amount that was added to the wallet.
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// Updated wallet balance after the top-up.
    /// </summary>
    public decimal NewBalance { get; init; }
    /// <summary>
    /// Processing status of the top-up (e.g. Completed, Failed).
    /// </summary>
    public string Status { get; init; } = string.Empty;
}

// —— Transfer ——
/// <summary>
/// Request DTO for transferring funds to another user's wallet by email.
/// </summary>
public record TransferRequestDto
{
    /// <summary>
    /// Email address of the transfer recipient.
    /// </summary>
    public string ToEmail { get; init; } = string.Empty;
    /// <summary>
    /// Amount to transfer from the sender's wallet.
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// Client-generated key to ensure idempotent processing.
    /// </summary>
    public string IdempotencyKey { get; init; } = string.Empty;
    /// <summary>
    /// Optional note or reason for the transfer.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// Response DTO returned after a successful transfer operation.
/// </summary>
public record TransferResponseDto
{
    /// <summary>
    /// Unique identifier of the transfer transaction.
    /// </summary>
    public Guid TransactionId { get; init; }
    /// <summary>
    /// Amount that was transferred.
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// Updated wallet balance of the sender after the transfer.
    /// </summary>
    public decimal NewBalance { get; init; }
    /// <summary>
    /// Processing status of the transfer (e.g. Completed, Failed).
    /// </summary>
    public string Status { get; init; } = string.Empty;
}

// —— Transaction History ——
/// <summary>
/// DTO representing a single ledger entry in the transaction history.
/// </summary>
public record TransactionDto
{
    /// <summary>
    /// Unique identifier of the ledger entry.
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// Transaction direction: CREDIT or DEBIT.
    /// </summary>
    public string Type { get; init; } = string.Empty;
    /// <summary>
    /// Monetary value of the transaction.
    /// </summary>
    public decimal Amount { get; init; }
    /// <summary>
    /// Identifier of the source record (TopUpRequest or TransferRequest).
    /// </summary>
    public Guid ReferenceId { get; init; }
    /// <summary>
    /// Category of the source record (e.g. TopUp, Transfer, Redemption).
    /// </summary>
    public string ReferenceType { get; init; } = string.Empty;
    /// <summary>
    /// Processing status derived from the referenced transaction record.
    /// </summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>
    /// Human-readable note attached to the transaction.
    /// </summary>
    public string Description { get; init; } = string.Empty;
    /// <summary>
    /// UTC timestamp when the ledger entry was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}

// —— Admin Financial Stats ——
/// <summary>
/// Aggregated financial statistics DTO used by the admin dashboard.
/// </summary>
public record WalletAdminStatsDto
{
    /// <summary>
    /// Total number of ledger entries across all wallets.
    /// </summary>
    public int TotalTransactionCount { get; init; }
    /// <summary>
    /// Cumulative credit volume across all wallets.
    /// </summary>
    public decimal TotalVolume { get; init; }
    /// <summary>
    /// Total credit volume processed today (UTC).
    /// </summary>
    public decimal TodaysVolume { get; init; }
    /// <summary>
    /// Number of ledger entries created today (UTC).
    /// </summary>
    public int TodaysTransactionCount { get; init; }
    /// <summary>
    /// Combined count of failed top-ups and failed transfers.
    /// </summary>
    public int FailedTransactions { get; init; }
    /// <summary>
    /// Mean transaction value calculated from total credit volume.
    /// </summary>
    public decimal AverageTransactionValue { get; init; }
}
