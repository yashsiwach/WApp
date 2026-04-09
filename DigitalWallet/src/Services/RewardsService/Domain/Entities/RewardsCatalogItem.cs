namespace RewardsService.Domain.Entities;

/// <summary>Item in the rewards catalog that users can redeem points for.</summary>
public class RewardsCatalogItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public string Category { get; set; } = string.Empty;    // Voucher | Cashback | Gift
    public bool IsAvailable { get; set; } = true;
    public int StockQuantity { get; set; } = -1;            // -1 = unlimited
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Redemption> Redemptions { get; set; } = new List<Redemption>();
}
