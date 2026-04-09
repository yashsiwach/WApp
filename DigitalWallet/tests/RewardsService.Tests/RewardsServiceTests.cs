using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using RewardsService.Application.DTOs;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Application.Options;
using RewardsService.Application.Services;
using RewardsService.Domain.Entities;
using SharedContracts.DTOs;
using SharedContracts.Events;

namespace RewardsService.Tests;

[TestFixture]
public class RewardsServiceTests
{
    private Mock<IUnitOfWork>                   _uowMock;
    private Mock<IRewardsAccountRepository>     _accountsMock;
    private Mock<IRewardsTransactionRepository> _transactionsMock;
    private Mock<IEarnRuleRepository>           _earnRulesMock;
    private Mock<ICatalogRepository>            _catalogMock;
    private Mock<IRedemptionRepository>         _redemptionsMock;
    private Mock<IPublishEndpoint>              _busMock;
    private Mock<ILogger<RewardsServiceImpl>>   _loggerMock;
    private RewardsServiceImpl                  _service;

    [SetUp]
    public void SetUp()
    {
        _accountsMock     = new Mock<IRewardsAccountRepository>();
        _transactionsMock = new Mock<IRewardsTransactionRepository>();
        _earnRulesMock    = new Mock<IEarnRuleRepository>();
        _catalogMock      = new Mock<ICatalogRepository>();
        _redemptionsMock  = new Mock<IRedemptionRepository>();
        _busMock          = new Mock<IPublishEndpoint>();
        _loggerMock       = new Mock<ILogger<RewardsServiceImpl>>();

        _uowMock = new Mock<IUnitOfWork>();
        _uowMock.Setup(u => u.Accounts).Returns(_accountsMock.Object);
        _uowMock.Setup(u => u.Transactions).Returns(_transactionsMock.Object);
        _uowMock.Setup(u => u.EarnRules).Returns(_earnRulesMock.Object);
        _uowMock.Setup(u => u.Catalog).Returns(_catalogMock.Object);
        _uowMock.Setup(u => u.Redemptions).Returns(_redemptionsMock.Object);

        var options = Options.Create(new RewardsOptions
        {
            Tiers =
            [
                new TierConfig { Tier = "Platinum", MinPoints = 10_000 },
                new TierConfig { Tier = "Gold",     MinPoints = 5_000  },
                new TierConfig { Tier = "Silver",   MinPoints = 1_000  },
                new TierConfig { Tier = "Bronze",   MinPoints = 0      },
            ]
        });

        _service = new RewardsServiceImpl(
            options,
            _uowMock.Object,
            _busMock.Object,
            _loggerMock.Object);
    }

    // ── GetAccountAsync ───────────────────────────────────────────────────────

    [Test]
    public async Task GetAccountAsync_WhenAccountExists_ReturnsDto()
    {
        var userId  = Guid.NewGuid();
        var account = new RewardsAccount { UserId = userId, PointsBalance = 500, Tier = "Bronze", LifetimePoints = 500 };
        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(account);

        var result = await _service.GetAccountAsync(userId);

        result.Should().NotBeNull();
        result.PointsBalance.Should().Be(500);
        result.Tier.Should().Be("Bronze");
    }

    [Test]
    public async Task GetAccountAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        _accountsMock.Setup(a => a.FindByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((RewardsAccount?)null);

        Func<Task> act = async () => await _service.GetAccountAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*not found*");
    }

    // ── GetCatalogAsync ───────────────────────────────────────────────────────

    [Test]
    public async Task GetCatalogAsync_ShouldDelegateToCatalogRepository()
    {
        var items = new List<CatalogItemDto> { new() { Id = Guid.NewGuid(), Name = "Coffee Voucher", PointsCost = 200 } };
        _catalogMock.Setup(c => c.GetAvailableAsync()).ReturnsAsync(items);

        var result = await _service.GetCatalogAsync();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Coffee Voucher");
    }

    // ── EarnPointsAsync ───────────────────────────────────────────────────────

    [Test]
    public async Task EarnPointsAsync_WhenNoActiveRule_ShouldDoNothing()
    {
        var userId  = Guid.NewGuid();
        var account = new RewardsAccount { UserId = userId, PointsBalance = 0 };
        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(account);
        _earnRulesMock.Setup(r => r.FindActiveByTriggerAsync("TopUp")).ReturnsAsync((EarnRule?)null);

        await _service.EarnPointsAsync(userId, 1000m, "TopUp", Guid.NewGuid(), "TopUp reward");

        _accountsMock.Verify(a => a.FindByUserIdAsync(userId), Times.Once);
        _busMock.Verify(b => b.Publish(It.IsAny<PointsEarned>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task EarnPointsAsync_WhenRuleExists_ShouldCreditPointsAndPublishEvent()
    {
        var userId  = Guid.NewGuid();
        var account = new RewardsAccount { UserId = userId, PointsBalance = 0, LifetimePoints = 0 };
        var rule    = new EarnRule { TriggerType = "TopUp", PointsPerRupee = 0.01m, IsActive = true };

        _earnRulesMock.Setup(r => r.FindActiveByTriggerAsync("TopUp")).ReturnsAsync(rule);
        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(account);

        await _service.EarnPointsAsync(userId, 1000m, "TopUp", Guid.NewGuid(), "TopUp reward");

        account.PointsBalance.Should().Be(10);   // 1000 * 0.01 = 10
        account.LifetimePoints.Should().Be(10);
        _uowMock.Verify(u => u.SaveAsync(), Times.Once);
        _busMock.Verify(b => b.Publish(It.Is<PointsEarned>(e => e.UserId == userId && e.Points == 10), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task EarnPointsAsync_WhenAccountDoesNotExist_ShouldCreateNewAccount()
    {
        var userId = Guid.NewGuid();
        var rule   = new EarnRule { TriggerType = "TopUp", PointsPerRupee = 1m, IsActive = true };

        _earnRulesMock.Setup(r => r.FindActiveByTriggerAsync("TopUp")).ReturnsAsync(rule);
        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync((RewardsAccount?)null);

        RewardsAccount? createdAccount = null;
        _accountsMock.Setup(a => a.AddAsync(It.IsAny<RewardsAccount>()))
                     .Callback<RewardsAccount>(a => { createdAccount = a; _accountsMock.Setup(x => x.FindByUserIdAsync(userId)).ReturnsAsync(a); });

        await _service.EarnPointsAsync(userId, 100m, "TopUp", Guid.NewGuid(), "TopUp reward");

        _accountsMock.Verify(a => a.AddAsync(It.IsAny<RewardsAccount>()), Times.Once);
    }

    // ── RedeemAsync ───────────────────────────────────────────────────────────

    [Test]
    public async Task RedeemAsync_WhenItemNotFound_ShouldThrowKeyNotFoundException()
    {
        var userId  = Guid.NewGuid();
        var account = new RewardsAccount { UserId = userId, PointsBalance = 1000 };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(account);
        _catalogMock.Setup(c => c.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((RewardsCatalogItem?)null);

        var tx = new Mock<IDbContextTransaction>();
        _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(tx.Object);

        Func<Task> act = async () => await _service.RedeemAsync(userId, new RedeemRequestDto { CatalogItemId = Guid.NewGuid() });

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Catalog item not found.");
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RedeemAsync_WhenItemNotAvailable_ShouldThrowInvalidOperation()
    {
        var userId  = Guid.NewGuid();
        var account = new RewardsAccount { UserId = userId, PointsBalance = 1000 };
        var item    = new RewardsCatalogItem { Id = Guid.NewGuid(), Name = "Gift", PointsCost = 200, IsAvailable = false };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(account);
        _catalogMock.Setup(c => c.FindByIdAsync(item.Id)).ReturnsAsync(item);

        var tx = new Mock<IDbContextTransaction>();
        _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(tx.Object);

        Func<Task> act = async () => await _service.RedeemAsync(userId, new RedeemRequestDto { CatalogItemId = item.Id });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*no longer available*");
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RedeemAsync_WhenInsufficientPoints_ShouldThrowInvalidOperation()
    {
        var userId  = Guid.NewGuid();
        var account = new RewardsAccount { UserId = userId, PointsBalance = 50 };
        var item    = new RewardsCatalogItem { Id = Guid.NewGuid(), Name = "Gift", PointsCost = 200, IsAvailable = true, StockQuantity = -1 };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(account);
        _catalogMock.Setup(c => c.FindByIdAsync(item.Id)).ReturnsAsync(item);

        var tx = new Mock<IDbContextTransaction>();
        _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(tx.Object);

        Func<Task> act = async () => await _service.RedeemAsync(userId, new RedeemRequestDto { CatalogItemId = item.Id });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Insufficient points*");
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RedeemAsync_ValidRequest_ShouldDeductPointsAndPublishEvent()
    {
        var userId  = Guid.NewGuid();
        var account = new RewardsAccount { UserId = userId, PointsBalance = 500 };
        var item    = new RewardsCatalogItem { Id = Guid.NewGuid(), Name = "Coffee", PointsCost = 200, IsAvailable = true, StockQuantity = -1 };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(account);
        _catalogMock.Setup(c => c.FindByIdAsync(item.Id)).ReturnsAsync(item);

        var tx = new Mock<IDbContextTransaction>();
        _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(tx.Object);

        var result = await _service.RedeemAsync(userId, new RedeemRequestDto { CatalogItemId = item.Id });

        result.Should().NotBeNull();
        result.PointsSpent.Should().Be(200);
        result.RemainingBalance.Should().Be(300);
        account.PointsBalance.Should().Be(300);
        tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _busMock.Verify(b => b.Publish(It.Is<RedemptionCompleted>(e => e.UserId == userId && e.PointsDeducted == 200), It.IsAny<CancellationToken>()), Times.Once);
    }
}
