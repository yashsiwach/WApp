using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedContracts.Events;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Consumers;

/// <summary>
/// Consumes KYCApproved events to unlock transfer features on wallets.
/// </summary>
public class KYCApprovedConsumer : IConsumer<KYCApproved>
{
    private readonly WalletDbContext _db;
    private readonly ILogger<KYCApprovedConsumer> _logger;

    public KYCApprovedConsumer(WalletDbContext db, ILogger<KYCApprovedConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<KYCApproved> context)
    {
        var msg = context.Message;

        var wallet = await _db.WalletAccounts.FirstOrDefaultAsync(w => w.UserId == msg.UserId);
        if (wallet == null)
        {
            _logger.LogWarning("Wallet not found for user {UserId} during KYC approval", msg.UserId);
            return;
        }

        wallet.KYCVerified = true;
        wallet.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("KYC verified for wallet {WalletId}, transfers unlocked", wallet.Id);
    }
}
