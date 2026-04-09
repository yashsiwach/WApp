namespace WalletService.Domain.Entities;

public class DailyLimitTracker
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public decimal TopUpTotal { get; set; } = 0m;
    public decimal TransferTotal { get; set; } = 0m;
    public int TransferCount { get; set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
