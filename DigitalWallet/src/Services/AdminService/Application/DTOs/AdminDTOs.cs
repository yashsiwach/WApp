namespace AdminService.Application.DTOs;

// ── KYC Review DTOs ──

/// <summary>
/// Read model representing a KYC document review for admin consumption.
/// </summary>
public record KYCReviewDto
{
    /// <summary>
    /// Unique identifier of the KYC review record.
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// Identifier of the underlying KYC document submitted by the user.
    /// </summary>
    public Guid DocumentId { get; init; }
    /// <summary>
    /// Identifier of the user who submitted the KYC document.
    /// </summary>
    public Guid UserId { get; init; }
    /// <summary>
    /// Type of identity document (e.g., Passport, NationalId).
    /// </summary>
    public string DocType { get; init; } = string.Empty;
    /// <summary>
    /// URL pointing to the uploaded document file.
    /// </summary>
    public string FileUrl { get; init; } = string.Empty;
    /// <summary>
    /// Current review status: Pending, Approved, or Rejected.
    /// </summary>
    public string Status { get; init; } = string.Empty;
    /// <summary>
    /// Optional notes or reason recorded by the reviewing admin.
    /// </summary>
    public string? ReviewNotes { get; init; }
    /// <summary>
    /// UTC timestamp when the document was originally submitted.
    /// </summary>
    public DateTime SubmittedAt { get; init; }
    /// <summary>
    /// UTC timestamp when an admin completed the review, or null if still pending.
    /// </summary>
    public DateTime? ReviewedAt { get; init; }
}

/// <summary>
/// Request payload for approving a pending KYC review.
/// </summary>
public record KYCApproveRequest
{
    /// <summary>
    /// Optional admin notes recorded alongside the approval decision.
    /// </summary>
    public string Notes { get; init; } = string.Empty;
}

/// <summary>
/// Request payload for rejecting a pending KYC review.
/// </summary>
public record KYCRejectRequest
{
    /// <summary>
    /// Reason provided by the admin explaining why the document was rejected.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}

// ── Dashboard DTOs ──

/// <summary>
/// Aggregated statistics shown on the admin dashboard.
/// </summary>
public record DashboardStatsDto
{
    /// <summary>
    /// Total number of KYC reviews currently awaiting admin action.
    /// </summary>
    public int PendingKYCCount { get; init; }
    /// <summary>
    /// Number of KYC reviews approved today (UTC day).
    /// </summary>
    public int ApprovedKYCToday { get; init; }
    /// <summary>
    /// Number of KYC reviews rejected today (UTC day).
    /// </summary>
    public int RejectedKYCToday { get; init; }
    /// <summary>
    /// Total admin actions logged today (UTC day).
    /// </summary>
    public int AdminActionsToday { get; init; }
}

// ── Rewards Catalog DTOs ──

/// <summary>
/// Request payload for adding a new item to the rewards catalog.
/// </summary>
public record CreateCatalogItemRequest
{
    /// <summary>
    /// Display name of the catalog item.
    /// </summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// Detailed description of the catalog item.
    /// </summary>
    public string Description { get; init; } = string.Empty;
    /// <summary>
    /// Number of reward points required to redeem this item.
    /// </summary>
    public int PointsCost { get; init; }
    /// <summary>
    /// Category grouping for the catalog item (e.g., Voucher, Merchandise).
    /// </summary>
    public string Category { get; init; } = string.Empty;
    /// <summary>
    /// Whether the item is currently available for redemption.
    /// </summary>
    public bool IsAvailable { get; init; } = true;
    /// <summary>
    /// Maximum redeemable stock; -1 indicates unlimited quantity.
    /// </summary>
    public int StockQuantity { get; init; } = -1;
}

/// <summary>
/// Request payload for updating an existing catalog item's details.
/// </summary>
public record UpdateCatalogItemRequest
{
    /// <summary>
    /// Updated display name of the catalog item.
    /// </summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// Updated description of the catalog item.
    /// </summary>
    public string Description { get; init; } = string.Empty;
    /// <summary>
    /// Updated points cost required to redeem the item.
    /// </summary>
    public int PointsCost { get; init; }
    /// <summary>
    /// Updated category grouping for the catalog item.
    /// </summary>
    public string Category { get; init; } = string.Empty;
    /// <summary>
    /// Updated availability flag for the catalog item.
    /// </summary>
    public bool IsAvailable { get; init; }
}

/// <summary>
/// Admin-facing read model for a rewards catalog item.
/// </summary>
public record CatalogItemAdminDto
{
    /// <summary>
    /// Unique identifier of the catalog item.
    /// </summary>
    public Guid Id { get; init; }
    /// <summary>
    /// Display name of the catalog item.
    /// </summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>
    /// Detailed description of the catalog item.
    /// </summary>
    public string Description { get; init; } = string.Empty;
    /// <summary>
    /// Points cost required to redeem the item.
    /// </summary>
    public int PointsCost { get; init; }
    /// <summary>
    /// Category grouping for the catalog item.
    /// </summary>
    public string Category { get; init; } = string.Empty;
    /// <summary>
    /// Whether the item is currently available for redemption.
    /// </summary>
    public bool IsAvailable { get; init; }
    /// <summary>
    /// Remaining stock quantity; -1 indicates unlimited.
    /// </summary>
    public int StockQuantity { get; init; }
}
