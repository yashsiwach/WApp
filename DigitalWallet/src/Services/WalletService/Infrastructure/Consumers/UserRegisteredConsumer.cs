using MassTransit;
using Microsoft.EntityFrameworkCore;
using SharedContracts.Events;
using WalletService.Domain.Entities;
using WalletService.Infrastructure.Data;

namespace WalletService.Infrastructure.Consumers;

/// <summary>
/// Consumes UserRegistered events to auto-create a wallet for new users.
/// </summary>
public class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly WalletDbContext _db;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(WalletDbContext db, ILogger<UserRegisteredConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var msg = context.Message;

        // Check if wallet already exists (idempotent)
        var exists = await _db.WalletAccounts.AnyAsync(w => w.UserId == msg.UserId);
        if (exists)
        {
            _logger.LogWarning("Wallet already exists for user {UserId}, skipping", msg.UserId);
            return;
        }

        var wallet = new WalletAccount
        {
            UserId = msg.UserId,
            Email = msg.Email.ToLower(),
            SnapshotBalance = 0m,
            Currency = "INR",
            IsLocked = false,
            KYCVerified = false
        };

        _db.WalletAccounts.Add(wallet);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Wallet created for user {UserId}: {WalletId}", msg.UserId, wallet.Id);
    }
}
