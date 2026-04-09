namespace WalletService.Domain.Entities;

public class LedgerEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public string Type { get; set; } = string.Empty; // CREDIT / DEBIT
    public decimal Amount { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceType { get; set; } = string.Empty; // TopUp, Transfer, Redemption
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public WalletAccount Wallet { get; set; } = null!;
}
