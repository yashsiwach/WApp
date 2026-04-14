using MassTransit;
using Microsoft.Extensions.Options;
using RewardsService.Application.DTOs;
using RewardsService.Application.Interfaces;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Application.Mappers;
using RewardsService.Application.Options;
using RewardsService.Domain.Entities;
using SharedContracts.DTOs;
using SharedContracts.Events;

namespace RewardsService.Application.Services;

public class RewardsServiceImpl : IRewardsQueryService, IRedemptionService, IPointsEarningService
{
    private readonly RewardsOptions _options;
    private readonly IUnitOfWork _uow;
    private readonly IPublishEndpoint _bus;
    private readonly ILogger<RewardsServiceImpl> _logger;

    /// <summary>Initializes rewards service dependencies for options, transactional data access, event publishing, and logging.</summary>
    public RewardsServiceImpl(
        IOptions<RewardsOptions> options,
        IUnitOfWork uow,
        IPublishEndpoint bus,
        ILogger<RewardsServiceImpl> logger)
    {
        _options = options.Value;
        _uow     = uow;
        _bus     = bus;
        _logger  = logger;
    }

    /// <summary>Returns the rewards account details for a user or throws when the account does not exist.</summary>
    public async Task<RewardsAccountDto> GetAccountAsync(Guid userId)
    {
        var account = await GetAccountOrThrow(userId);
        return RewardsMapper.ToDto(account);
    }

    /// <summary>Returns paginated rewards transactions for the user's rewards account.</summary>
    public async Task<PaginatedResult<RewardsTransactionDto>> GetTransactionsAsync(Guid userId, int page, int size)
    {
        var account = await GetAccountOrThrow(userId);
        return await _uow.Transactions.GetPagedAsync(account.Id, page, size);
    }

    /// <summary>Returns all currently available catalog items for redemption.</summary>
    public Task<List<CatalogItemDto>> GetCatalogAsync() =>_uow.Catalog.GetAvailableAsync();

    /// <summary>Validates redemption rules, deducts points, records transaction and redemption, publishes event, and returns response.</summary>
    public async Task<RedeemResponseDto> RedeemAsync(Guid userId, RedeemRequestDto request)
    {
        var account = await GetAccountOrThrow(userId);
        var item    = await _uow.Catalog.FindByIdAsync(request.CatalogItemId)?? throw new KeyNotFoundException("Catalog item not found.");

        if (!item.IsAvailable)        throw new InvalidOperationException("This item is no longer available.");
        if (item.StockQuantity == 0)  throw new InvalidOperationException("Item is out of stock.");

        await using var tx = await _uow.BeginTransactionAsync();
        try
        {
            // Validate that the user has enough points before making any changes
            if (account.PointsBalance < item.PointsCost)
                throw new InvalidOperationException($"Insufficient points. You need {item.PointsCost}, you have {account.PointsBalance}.");

            // Deduct points and record the transaction and redemption within the open transaction
            account.PointsBalance -= item.PointsCost;
            account.UpdatedAt      = DateTime.UtcNow;

            await _uow.Transactions.AddAsync(new RewardsTransaction
            {
                RewardsAccountId = account.Id,
                Type             = "REDEEM",
                PointsDelta      = -item.PointsCost,
                ReferenceType    = "Redemption",
                Description      = $"Redeemed: {item.Name}"
            });

            var fulfillmentCode = Guid.NewGuid().ToString("N").ToUpper()[..12];
            var redemption = new Redemption
            {
                RewardsAccountId = account.Id,
                CatalogItemId    = item.Id,
                PointsSpent      = item.PointsCost,
                Status           = "Completed",
                FulfillmentCode  = fulfillmentCode
            };
            await _uow.Redemptions.AddAsync(redemption);

            // Decrement finite stock and mark item unavailable when it reaches zero
            if (item.StockQuantity > 0)
            {
                item.StockQuantity -= 1;
                if (item.StockQuantity == 0) item.IsAvailable = false;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _uow.SaveAsync();
            await tx.CommitAsync();

            _logger.LogInformation("Redemption {Id} completed for user {UserId}: {Item} ({Points}pts)",
                redemption.Id, userId, item.Name, item.PointsCost);

            // Fire-and-forget: redemption is committed, notification doesn't block the response
            _ = _bus.Publish(new RedemptionCompleted
            {
                UserId           = userId,
                RedemptionId     = redemption.Id,
                ItemName         = item.Name,
                Category         = item.Category,
                PointsDeducted   = item.PointsCost,
                CouponCode       = fulfillmentCode,
                RemainingBalance = account.PointsBalance,
                OccurredAt       = DateTime.UtcNow
            }).ContinueWith(t => _logger.LogError(t.Exception, "Failed to publish RedemptionCompleted {Id}", redemption.Id),
                TaskContinuationOptions.OnlyOnFaulted);

            return RewardsMapper.ToRedeemResponse(redemption, account.PointsBalance);
        }
        catch (Exception)
        {
            try { await tx.RollbackAsync(); } catch (Exception rollbackEx) { _logger.LogError(rollbackEx, "Rollback failed during redemption"); }
            throw;
        }
    }

    /// <summary>Returns the redemption history for the user's rewards account.</summary>
    public async Task<List<RedemptionDto>> GetRedemptionsAsync(Guid userId)
    {
        var account = await GetAccountOrThrow(userId);
        return await _uow.Redemptions.GetByAccountIdAsync(account.Id);
    }

    /// <summary>Awards points from a trigger event, updates account totals and tier, records transaction, and publishes points earned event.</summary>
    public async Task EarnPointsAsync(Guid userId, decimal amount, string triggerType, Guid referenceId, string description)
    {
        // Always ensure the rewards account exists — even if no points will be awarded.
        // This handles the registration flow where amount=0 is passed just to create the account.
        var account = await _uow.Accounts.FindByUserIdAsync(userId);
        if (account == null)
        {
            account = new RewardsAccount { UserId = userId };
            await _uow.Accounts.AddAsync(account);
            await _uow.SaveAsync();
            _logger.LogInformation("Rewards account created for user {UserId}", userId);
        }

        var rule = await _uow.EarnRules.FindActiveByTriggerAsync(triggerType);
        if (rule == null)
        {
            _logger.LogWarning("No active earn rule for '{TriggerType}', skipping points award", triggerType);
            return;
        }

        // Idempotency: skip awarding if the calculated points round to zero or below
        var pointsEarned = (int)Math.Round(amount * rule.PointsPerRupee);
        if (pointsEarned <= 0) return;

        // Recalculate tier after updating lifetime points to reflect any threshold crossing
        account.PointsBalance  += pointsEarned;
        account.LifetimePoints += pointsEarned;
        account.Tier            = CalculateTier(account.LifetimePoints);
        account.UpdatedAt       = DateTime.UtcNow;

        await _uow.Transactions.AddAsync(new RewardsTransaction
        {
            RewardsAccountId = account.Id,
            Type             = "EARN",
            PointsDelta      = pointsEarned,
            ReferenceType    = triggerType,
            ReferenceId      = referenceId,
            Description      = description
        });

        await _uow.SaveAsync();

        _logger.LogInformation(
            "Earned {Points}pts for user {UserId} via {TriggerType}",
            pointsEarned,
            userId,
            triggerType);

        // Fire-and-forget: points are saved, notification doesn't block the caller
        _ = _bus.Publish(new PointsEarned
        {
            UserId           = userId,
            RewardsAccountId = account.Id,
            Points           = pointsEarned,
            Reason           = description,
            Tier             = account.Tier,
            NewBalance       = account.PointsBalance,
            OccurredAt       = DateTime.UtcNow
        }).ContinueWith(t => _logger.LogError(t.Exception, "Failed to publish PointsEarned for user {UserId}", userId),
            TaskContinuationOptions.OnlyOnFaulted);
    }

    /// <summary>
    /// Returns the rewards account for a user, creating it on first access if it wasn't
    /// provisioned by the UserRegistered event (e.g. RabbitMQ was down or event was missed).
    /// </summary>
    private async Task<RewardsAccount> GetAccountOrThrow(Guid userId)
    {
        var account = await _uow.Accounts.FindByUserIdAsync(userId);
        if (account != null) return account;

        account = new RewardsAccount { UserId = userId };
        await _uow.Accounts.AddAsync(account);
        await _uow.SaveAsync();
        _logger.LogInformation("Rewards account auto-provisioned for user {UserId} on first access", userId);
        return account;
    }

    /// <summary>Calculates the reward tier based on lifetime points using configured tier thresholds.</summary>
    private string CalculateTier(int pts) =>
        _options.Tiers.OrderByDescending(t => t.MinPoints).FirstOrDefault(t => pts >= t.MinPoints)?.Tier ?? "Bronze";
}
