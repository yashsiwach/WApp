namespace SharedContracts.Enums;

/// <summary>
/// Defines the access role assigned to a registered user.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Standard end-user with wallet and payment access.
    /// </summary>
    User = 0,
    /// <summary>
    /// Administrator with full system management privileges.
    /// </summary>
    Admin = 1,
    /// <summary>
    /// Support team member with access to customer service tools.
    /// </summary>
    SupportAgent = 2
}
