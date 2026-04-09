namespace RewardsService.Domain.Entities;

/// <summary>Records a user redemption of catalog items using points.</summary>
public class Redemption
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RewardsAccountId { get; set; }
    public Guid CatalogItemId { get; set; }
    public int PointsSpent { get; set; }
    public string Status { get; set; } = "Pending";        // Pending | Completed | Cancelled
    public string? FulfillmentCode { get; set; }           // voucher code / gift card etc.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public RewardsAccount RewardsAccount { get; set; } = null!;
    public RewardsCatalogItem CatalogItem { get; set; } = null!;
}
