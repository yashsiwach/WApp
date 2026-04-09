namespace AdminService.Application.DTOs;

// ── KYC Review DTOs ──

public record KYCReviewDto
{
    public Guid Id { get; init; }
    public Guid DocumentId { get; init; }
    public Guid UserId { get; init; }
    public string DocType { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? ReviewNotes { get; init; }
    public DateTime SubmittedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
}

public record KYCApproveRequest
{
    public string Notes { get; init; } = string.Empty;
}

public record KYCRejectRequest
{
    public string Reason { get; init; } = string.Empty;
}

// ── Dashboard DTOs ──

public record DashboardStatsDto
{
    public int PendingKYCCount { get; init; }
    public int ApprovedKYCToday { get; init; }
    public int RejectedKYCToday { get; init; }
    public int AdminActionsToday { get; init; }
}

// ── Rewards Catalog DTOs ──

public record CreateCatalogItemRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int PointsCost { get; init; }
    public string Category { get; init; } = string.Empty;
    public bool IsAvailable { get; init; } = true;
    public int StockQuantity { get; init; } = -1;
}

public record CatalogItemAdminDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int PointsCost { get; init; }
    public string Category { get; init; } = string.Empty;
    public bool IsAvailable { get; init; }
    public int StockQuantity { get; init; }
}
