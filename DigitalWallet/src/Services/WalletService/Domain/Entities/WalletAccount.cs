namespace WalletService.Domain.Entities;

public class WalletAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public decimal SnapshotBalance { get; set; } = 0m;
    public string Currency { get; set; } = "INR";
    public bool IsLocked { get; set; } = false;
    public bool KYCVerified { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<LedgerEntry> LedgerEntries { get; set; } = new List<LedgerEntry>();
    public ICollection<TopUpRequest> TopUpRequests { get; set; } = new List<TopUpRequest>();
}
