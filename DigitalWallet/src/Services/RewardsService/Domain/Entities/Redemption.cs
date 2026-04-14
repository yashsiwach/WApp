namespace RewardsService.Domain.Entities;

/// <summary>Records a user redemption of catalog items using points.</summary>
public class Redemption
{
    /// <summary>
    /// Unique identifier of the redemption record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Foreign key referencing the rewards account that made this redemption.
    /// </summary>
    public Guid RewardsAccountId { get; set; }
    /// <summary>
    /// Foreign key referencing the catalog item that was redeemed.
    /// </summary>
    public Guid CatalogItemId { get; set; }
    /// <summary>
    /// Number of points deducted for this redemption.
    /// </summary>
    public int PointsSpent { get; set; }
    /// <summary>
    /// Current lifecycle status of the redemption (Pending, Completed, Cancelled).
    /// </summary>
    public string Status { get; set; } = "Pending";        // Pending | Completed | Cancelled
    /// <summary>
    /// Voucher or gift-card code issued upon fulfilment, if applicable.
    /// </summary>
    public string? FulfillmentCode { get; set; }           // voucher code / gift card etc.
    /// <summary>
    /// UTC timestamp when the redemption was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>
    /// UTC timestamp when the redemption record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property to the owning rewards account.
    /// </summary>
    public RewardsAccount RewardsAccount { get; set; } = null!;
    /// <summary>
    /// Navigation property to the redeemed catalog item.
    /// </summary>
    public RewardsCatalogItem CatalogItem { get; set; } = null!;
}
