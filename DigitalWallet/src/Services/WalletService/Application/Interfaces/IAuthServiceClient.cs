namespace WalletService.Application.Interfaces;

public interface IAuthServiceClient
{
    /// <summary>Calls AuthService to resolve a user's ID from their email address.</summary>
    Task<Guid?> GetUserIdByEmailAsync(string email, string bearerToken);
}
