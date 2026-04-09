using MassTransit;
using Microsoft.Extensions.Options;
using SharedContracts.DTOs;
using SharedContracts.Events;
using WalletService.Application.DTOs;
using WalletService.Application.Interfaces;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Mappers;
using WalletService.Application.Options;
using WalletService.Domain.Entities;

namespace WalletService.Application.Services;

public class WalletServiceImpl : IWalletQueryService, IWalletCommandService
{
    private readonly WalletOptions _limits;
    private readonly IUnitOfWork _uow;
    private readonly IPublishEndpoint _bus;
    private readonly IAuthServiceClient _authClient;
    private readonly ILogger<WalletServiceImpl> _logger;

    /// <summary>Initializes wallet service dependencies for limit options, transactional data access, events, and logging.</summary>
    public WalletServiceImpl(
        IOptions<WalletOptions> limits,
        IUnitOfWork uow,
        IPublishEndpoint bus,
        IAuthServiceClient authClient,
        ILogger<WalletServiceImpl> logger)
    {
        _limits     = limits.Value;
        _uow        = uow;
        _bus        = bus;
        _authClient = authClient;
        _logger     = logger;
    }

    // —— GET BALANCE ——

    /// <summary>Returns current wallet balance details for the given user.</summary>
    public async Task<BalanceResponse> GetBalanceAsync(Guid userId)
    {
        var wallet = await GetWalletOrThrow(userId);
        return WalletMapper.ToBalanceResponse(wallet);
    }

    // —— TOP-UP ——

    /// <summary>Processes a wallet top-up with idempotency and daily limit checks, persists ledger changes, and publishes completion or failure events.</summary>
    public async Task<TopUpResponseDto> TopUpAsync(Guid userId, TopUpRequestDto request)
    {
        var wallet = await GetWalletOrThrow(userId);

        if (wallet.IsLocked)
            throw new InvalidOperationException("Wallet is locked. Contact support.");

        var existing = await _uow.TopUps.FindByIdempotencyKeyAsync(request.IdempotencyKey);
        if (existing != null)
            return WalletMapper.ToTopUpResponse(existing, wallet.SnapshotBalance);

        await using var tx = await _uow.BeginTransactionAsync();
        try
        {
            var tracker = await GetOrCreateDailyTracker(wallet.Id);
            if (tracker.TopUpTotal + request.Amount > _limits.DailyTopUpLimit)
                throw new InvalidOperationException($"Daily top-up limit of â‚¹{_limits.DailyTopUpLimit:N2} would be exceeded.");

            var topUp = new TopUpRequest
            {
                WalletId       = wallet.Id,
                Amount         = request.Amount,
                Provider       = request.Provider,
                IdempotencyKey = request.IdempotencyKey,
                Status         = "Completed"
            };
            await _uow.TopUps.AddAsync(topUp);

            await _uow.Ledger.AddAsync(new LedgerEntry
            {
                WalletId      = wallet.Id,
                Type          = "CREDIT",
                Amount        = request.Amount,
                ReferenceId   = topUp.Id,
                ReferenceType = "TopUp",
                Description   = $"Top-up via {request.Provider}"
            });

            wallet.SnapshotBalance += request.Amount;
            wallet.UpdatedAt        = DateTime.UtcNow;
            tracker.TopUpTotal     += request.Amount;
            tracker.UpdatedAt       = DateTime.UtcNow;

            await _uow.SaveAsync();
            await tx.CommitAsync();

            _logger.LogInformation("TopUp completed: {TopUpId} â‚¹{Amount} for wallet {WalletId}", topUp.Id, request.Amount, wallet.Id);

            // Fire-and-forget: transaction is committed, rewards earning doesn't block the response
            _ = _bus.Publish(new TopUpCompleted
            {
                UserId         = userId,
                WalletId       = wallet.Id,
                TransactionId  = topUp.Id,
                Amount         = request.Amount,
                Currency       = wallet.Currency,
                Provider       = request.Provider,
                IdempotencyKey = request.IdempotencyKey,
                OccurredAt     = DateTime.UtcNow
            }).ContinueWith(t => _logger.LogError(t.Exception, "Failed to publish TopUpCompleted {TopUpId}", topUp.Id),
                TaskContinuationOptions.OnlyOnFaulted);

            return WalletMapper.ToTopUpResponse(topUp, wallet.SnapshotBalance);
        }
        catch
        {
            await tx.RollbackAsync();
            _ = _bus.Publish(new PaymentFailed { UserId = userId, WalletId = wallet.Id, TransactionId = Guid.Empty, Amount = request.Amount, Reason = "Top-up processing failed", TransactionType = "TopUp", OccurredAt = DateTime.UtcNow });
            throw;
        }
    }

    // —— TRANSFER ——

    /// <summary>Transfers funds between wallets with idempotency, KYC, balance, and daily limit validations, then publishes transfer events.</summary>
    public async Task<TransferResponseDto> TransferAsync(Guid userId, TransferRequestDto request, string bearerToken)
    {
        var senderWallet = await GetWalletOrThrow(userId);

        if (senderWallet.IsLocked)       throw new InvalidOperationException("Your wallet is locked. Contact support.");
        if (!senderWallet.KYCVerified)   throw new InvalidOperationException("KYC verification required to make transfers.");

        var existing = await _uow.Transfers.FindByIdempotencyKeyAsync(request.IdempotencyKey);
        if (existing != null)
            return WalletMapper.ToTransferResponse(existing, senderWallet.SnapshotBalance);

        // Try direct email lookup first (works for wallets created after the email column was added).
        // Fall back to resolving via AuthService for wallets created before the column existed.
        var receiverWallet = await _uow.WalletAccounts.FindByEmailAsync(request.ToEmail);
        if (receiverWallet == null)
        {
            var recipientUserId = await _authClient.GetUserIdByEmailAsync(request.ToEmail, bearerToken);
            if (recipientUserId.HasValue)
                receiverWallet = await _uow.WalletAccounts.FindByUserIdAsync(recipientUserId.Value);
        }
        if (receiverWallet == null)
            throw new KeyNotFoundException("No wallet found for the recipient email.");

        if (receiverWallet.IsLocked)
            throw new InvalidOperationException("Recipient wallet is locked.");

        await using var tx = await _uow.BeginTransactionAsync();
        try
        {
            var tracker = await GetOrCreateDailyTracker(senderWallet.Id);
            if (tracker.TransferCount >= _limits.MaxDailyTransfers)
                throw new InvalidOperationException($"Maximum {_limits.MaxDailyTransfers} transfers per day exceeded.");
            if (tracker.TransferTotal + request.Amount > _limits.DailyTransferLimit)
                throw new InvalidOperationException($"Daily transfer limit of â‚¹{_limits.DailyTransferLimit:N2} would be exceeded.");
            if (senderWallet.SnapshotBalance < request.Amount)
                throw new InvalidOperationException("Insufficient balance.");

            var transfer = new TransferRequest
            {
                FromWalletId   = senderWallet.Id,
                ToWalletId     = receiverWallet.Id,
                Amount         = request.Amount,
                IdempotencyKey = request.IdempotencyKey,
                Status         = "Completed"
            };
            await _uow.Transfers.AddAsync(transfer);

            await _uow.Ledger.AddAsync(new LedgerEntry { WalletId = senderWallet.Id,   Type = "DEBIT",  Amount = request.Amount, ReferenceId = transfer.Id, ReferenceType = "Transfer", Description = request.Description ?? "Transfer" });
            await _uow.Ledger.AddAsync(new LedgerEntry { WalletId = receiverWallet.Id, Type = "CREDIT", Amount = request.Amount, ReferenceId = transfer.Id, ReferenceType = "Transfer", Description = "Received transfer" });

            senderWallet.SnapshotBalance   -= request.Amount;
            senderWallet.UpdatedAt          = DateTime.UtcNow;
            receiverWallet.SnapshotBalance += request.Amount;
            receiverWallet.UpdatedAt        = DateTime.UtcNow;
            tracker.TransferTotal          += request.Amount;
            tracker.TransferCount          += 1;
            tracker.UpdatedAt               = DateTime.UtcNow;

            await _uow.SaveAsync();
            await tx.CommitAsync();

            _logger.LogInformation("Transfer completed: {TransferId} â‚¹{Amount} from {From} to {To}", transfer.Id, request.Amount, senderWallet.Id, receiverWallet.Id);

            // Fire-and-forget: transaction is committed, rewards earning doesn't block the response
            _ = _bus.Publish(new TransferCompleted
            {
                FromUserId     = userId,
                ToUserId       = receiverWallet.UserId,
                FromWalletId   = senderWallet.Id,
                ToWalletId     = receiverWallet.Id,
                TransactionId  = transfer.Id,
                Amount         = request.Amount,
                Currency       = senderWallet.Currency,
                IdempotencyKey = request.IdempotencyKey,
                OccurredAt     = DateTime.UtcNow
            }).ContinueWith(t => _logger.LogError(t.Exception, "Failed to publish TransferCompleted {TransferId}", transfer.Id),
                TaskContinuationOptions.OnlyOnFaulted);

            return WalletMapper.ToTransferResponse(transfer, senderWallet.SnapshotBalance);
        }
        catch
        {
            await tx.RollbackAsync();
            _ = _bus.Publish(new PaymentFailed { UserId = userId, WalletId = senderWallet.Id, TransactionId = Guid.Empty, Amount = request.Amount, Reason = "Transfer processing failed", TransactionType = "Transfer", OccurredAt = DateTime.UtcNow });
            throw;
        }
    }

    // —— TRANSACTIONS ——

    /// <summary>Returns paginated wallet ledger transactions for a user with optional date range filtering.</summary>
    public async Task<PaginatedResult<TransactionDto>> GetTransactionsAsync(Guid userId, int page, int size, DateTime? from, DateTime? to)
    {
        var wallet = await GetWalletOrThrow(userId);
        return await _uow.Ledger.GetPagedAsync(wallet.Id, page, size, from, to);
    }

    // —— HELPERS ——

    /// <summary>
    /// Returns the wallet for a user, creating it on first access if it wasn't provisioned
    /// by the UserRegistered event (e.g. RabbitMQ was down or event was missed).
    /// </summary>
    private async Task<WalletAccount> GetWalletOrThrow(Guid userId)
    {
        var wallet = await _uow.WalletAccounts.FindByUserIdAsync(userId);
        if (wallet != null) return wallet;

        wallet = new WalletAccount
        {
            UserId          = userId,
            SnapshotBalance = 0m,
            Currency        = "INR",
            IsLocked        = false,
            KYCVerified     = false
        };
        await _uow.WalletAccounts.AddAsync(wallet);
        await _uow.SaveAsync();
        _logger.LogInformation("Wallet auto-provisioned for user {UserId} on first access", userId);
        return wallet;
    }

    /// <summary>Gets today's daily limit tracker for a wallet or creates it safely under concurrent requests.</summary>
    private async Task<DailyLimitTracker> GetOrCreateDailyTracker(Guid walletId)
    {
        var today   = DateTime.UtcNow.Date;
        var tracker = await _uow.DailyLimits.FindByWalletAndDateAsync(walletId, today);
        if (tracker != null) return tracker;

        tracker = new DailyLimitTracker { WalletId = walletId, Date = today };
        await _uow.DailyLimits.AddAsync(tracker);
        try
        {
            await _uow.SaveAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // Another concurrent request already inserted the tracker — re-query it
            tracker = await _uow.DailyLimits.FindByWalletAndDateAsync(walletId, today)
                ?? throw new InvalidOperationException("Failed to retrieve daily limit tracker.");
        }
        return tracker;
    }
}
