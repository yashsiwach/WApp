namespace SharedContracts.Enums;

/// <summary>
/// Represents the processing state of a wallet transaction.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transaction has been initiated and is awaiting processing.
    /// </summary>
    Pending = 0,
    /// <summary>
    /// Transaction has been successfully processed.
    /// </summary>
    Completed = 1,
    /// <summary>
    /// Transaction could not be completed due to an error.
    /// </summary>
    Failed = 2
}
