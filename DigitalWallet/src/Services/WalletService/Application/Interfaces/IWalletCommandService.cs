using WalletService.Application.DTOs;

namespace WalletService.Application.Interfaces;

/// <summary>State-changing wallet operations.</summary>
public interface IWalletCommandService
{
    /// <summary>
    /// Adds funds to the specified user's wallet using the provided top-up details.
    /// </summary>
    Task<TopUpResponseDto> TopUpAsync(Guid userId, TopUpRequestDto request);
    /// <summary>
    /// Transfers funds from the specified user's wallet to another wallet identified by email.
    /// </summary>
    Task<TransferResponseDto> TransferAsync(Guid userId, TransferRequestDto request, string bearerToken);
}
