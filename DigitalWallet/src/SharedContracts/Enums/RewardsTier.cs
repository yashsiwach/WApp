namespace SharedContracts.Enums;

/// <summary>
/// Represents a user's loyalty tier based on accumulated reward points.
/// </summary>
public enum RewardsTier
{
    /// <summary>
    /// Entry-level tier for users with 0–999 points.
    /// </summary>
    Silver = 0,    // 0–999 points
    /// <summary>
    /// Mid-level tier for users with 1000–4999 points.
    /// </summary>
    Gold = 1,      // 1000–4999 points
    /// <summary>
    /// Top-level tier for users with 5000 or more points.
    /// </summary>
    Platinum = 2   // 5000+ points
}
