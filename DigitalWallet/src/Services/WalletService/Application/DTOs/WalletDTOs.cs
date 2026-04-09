namespace WalletService.Application.DTOs;

// —— Balance ——
public record BalanceResponse
{
    public Guid WalletId { get; init; }
    public decimal Balance { get; init; }
    public string Currency { get; init; } = "INR";
    public bool IsLocked { get; init; }
    public bool KYCVerified { get; init; }
}

// —— TopUp ——
public record TopUpRequestDto
{
    public decimal Amount { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string IdempotencyKey { get; init; } = string.Empty;
}

public record TopUpResponseDto
{
    public Guid TransactionId { get; init; }
    public decimal Amount { get; init; }
    public decimal NewBalance { get; init; }
    public string Status { get; init; } = string.Empty;
}

// —— Transfer ——
public record TransferRequestDto
{
    public string ToEmail { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string IdempotencyKey { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record TransferResponseDto
{
    public Guid TransactionId { get; init; }
    public decimal Amount { get; init; }
    public decimal NewBalance { get; init; }
    public string Status { get; init; } = string.Empty;
}

// —— Transaction History ——
public record TransactionDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public Guid ReferenceId { get; init; }
    public string ReferenceType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
