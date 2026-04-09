using WalletService.Application.DTOs;

namespace WalletService.Application.Interfaces;

/// <summary>State-changing wallet operations.</summary>
public interface IWalletCommandService
{
    Task<TopUpResponseDto> TopUpAsync(Guid userId, TopUpRequestDto request);
    Task<TransferResponseDto> TransferAsync(Guid userId, TransferRequestDto request, string bearerToken);
}
