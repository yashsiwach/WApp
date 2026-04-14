namespace SharedContracts.Enums;

/// <summary>
/// Represents the direction of a ledger entry in the wallet transaction log.
/// </summary>
public enum LedgerEntryType
{
    /// <summary>
    /// Funds added to the wallet balance.
    /// </summary>
    Credit = 0,
    /// <summary>
    /// Funds deducted from the wallet balance.
    /// </summary>
    Debit = 1
}
