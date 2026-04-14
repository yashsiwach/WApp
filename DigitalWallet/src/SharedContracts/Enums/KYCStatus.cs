namespace SharedContracts.Enums;

/// <summary>
/// Represents the verification status of a user's KYC submission.
/// </summary>
public enum KYCStatus
{
    /// <summary>
    /// KYC document has been submitted and is awaiting review.
    /// </summary>
    Pending = 0,
    /// <summary>
    /// KYC document has been reviewed and approved.
    /// </summary>
    Approved = 1,
    /// <summary>
    /// KYC document has been reviewed and rejected.
    /// </summary>
    Rejected = 2
}
