using WalletService.Application.DTOs;
using WalletService.Domain.Entities;

namespace WalletService.Application.Mappers;

/// <summary>Single responsibility: maps WalletService domain objects to DTOs.</summary>
public static class WalletMapper
{
    /// <summary>
    /// Maps a WalletAccount entity to a BalanceResponse DTO.
    /// </summary>
    public static BalanceResponse ToBalanceResponse(WalletAccount wallet) => new()
    {
        WalletId    = wallet.Id,
        Balance     = wallet.SnapshotBalance,
        Currency    = wallet.Currency,
        IsLocked    = wallet.IsLocked,
        KYCVerified = wallet.KYCVerified
    };

    /// <summary>
    /// Maps a TopUpRequest entity and the resulting balance to a TopUpResponseDto.
    /// </summary>
    public static TopUpResponseDto ToTopUpResponse(TopUpRequest topUp, decimal newBalance) => new()
    {
        TransactionId = topUp.Id,
        Amount        = topUp.Amount,
        NewBalance    = newBalance,
        Status        = topUp.Status
    };

    /// <summary>
    /// Maps a TransferRequest entity and the sender's resulting balance to a TransferResponseDto.
    /// </summary>
    public static TransferResponseDto ToTransferResponse(TransferRequest transfer, decimal newBalance) => new()
    {
        TransactionId = transfer.Id,
        Amount        = transfer.Amount,
        NewBalance    = newBalance,
        Status        = transfer.Status
    };
}
