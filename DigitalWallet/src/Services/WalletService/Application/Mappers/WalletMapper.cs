using WalletService.Application.DTOs;
using WalletService.Domain.Entities;

namespace WalletService.Application.Mappers;

/// <summary>Single responsibility: maps WalletService domain objects to DTOs.</summary>
public static class WalletMapper
{
    public static BalanceResponse ToBalanceResponse(WalletAccount wallet) => new()
    {
        WalletId    = wallet.Id,
        Balance     = wallet.SnapshotBalance,
        Currency    = wallet.Currency,
        IsLocked    = wallet.IsLocked,
        KYCVerified = wallet.KYCVerified
    };

    public static TopUpResponseDto ToTopUpResponse(TopUpRequest topUp, decimal newBalance) => new()
    {
        TransactionId = topUp.Id,
        Amount        = topUp.Amount,
        NewBalance    = newBalance,
        Status        = topUp.Status
    };

    public static TransferResponseDto ToTransferResponse(TransferRequest transfer, decimal newBalance) => new()
    {
        TransactionId = transfer.Id,
        Amount        = transfer.Amount,
        NewBalance    = newBalance,
        Status        = transfer.Status
    };
}
