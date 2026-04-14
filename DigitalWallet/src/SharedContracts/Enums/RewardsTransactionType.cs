namespace SharedContracts.Enums;

/// <summary>
/// Describes the nature of a rewards points transaction.
/// </summary>
public enum RewardsTransactionType
{
    /// <summary>
    /// Points credited to the user's rewards account.
    /// </summary>
    Earn = 0,
    /// <summary>
    /// Points redeemed by the user for a reward or voucher.
    /// </summary>
    Spend = 1,
    /// <summary>
    /// Points removed from the account due to expiry.
    /// </summary>
    Expire = 2
}
