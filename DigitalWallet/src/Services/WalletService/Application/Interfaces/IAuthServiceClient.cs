namespace WalletService.Application.Interfaces;

/// <summary>
/// Contract for communicating with the AuthService to resolve user identities.
/// </summary>
public interface IAuthServiceClient
{
    /// <summary>Calls AuthService to resolve a user's ID from their email address.</summary>
    Task<Guid?> GetUserIdByEmailAsync(string email, string bearerToken);
}
