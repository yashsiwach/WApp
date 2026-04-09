using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SharedContracts.DTOs;
using SharedContracts.Events;
using WalletService.Application.DTOs;
using WalletService.Application.Interfaces;
using WalletService.Application.Interfaces.Repositories;
using WalletService.Application.Options;
using WalletService.Application.Services;
using WalletService.Domain.Entities;

namespace WalletService.Tests;

[TestFixture]
public class WalletServiceImplTests
{
    private Mock<IUnitOfWork> _uowMock;
    private Mock<IWalletAccountRepository> _accountsMock;
    private Mock<ILedgerRepository> _ledgerMock;
    private Mock<ITopUpRepository> _topUpsMock;
    private Mock<ITransferRepository> _transfersMock;
    private Mock<IDailyLimitRepository> _dailyLimitsMock;
    private Mock<IPublishEndpoint> _busMock;
    private Mock<IAuthServiceClient> _authClientMock;
    private Mock<ILogger<WalletServiceImpl>> _loggerMock;
    private WalletServiceImpl _service;

    [SetUp]
    public void SetUp()
    {
        _accountsMock = new Mock<IWalletAccountRepository>();
        _ledgerMock = new Mock<ILedgerRepository>();
        _topUpsMock = new Mock<ITopUpRepository>();
        _transfersMock = new Mock<ITransferRepository>();
        _dailyLimitsMock = new Mock<IDailyLimitRepository>();
        _busMock = new Mock<IPublishEndpoint>();
        _authClientMock = new Mock<IAuthServiceClient>();
        _loggerMock = new Mock<ILogger<WalletServiceImpl>>();

        _uowMock = new Mock<IUnitOfWork>();
        _uowMock.Setup(u => u.WalletAccounts).Returns(_accountsMock.Object);
        _uowMock.Setup(u => u.Ledger).Returns(_ledgerMock.Object);
        _uowMock.Setup(u => u.TopUps).Returns(_topUpsMock.Object);
        _uowMock.Setup(u => u.Transfers).Returns(_transfersMock.Object);
        _uowMock.Setup(u => u.DailyLimits).Returns(_dailyLimitsMock.Object);

        var options = Options.Create(new WalletOptions
        {
            DailyTopUpLimit = 50000m,
            DailyTransferLimit = 25000m,
            MaxDailyTransfers = 10
        });

        _service = new WalletServiceImpl(options, _uowMock.Object, _busMock.Object, _authClientMock.Object, _loggerMock.Object);
    }

    // ── GetBalanceAsync ───────────────────────────────────────────────────────

    [Test]
    public async Task GetBalanceAsync_WhenWalletExists_ReturnsBalance()
    {
        var userId = Guid.NewGuid();
        var wallet = new WalletAccount { Id = Guid.NewGuid(), UserId = userId, SnapshotBalance = 1000m, Currency = "INR" };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(wallet);

        var result = await _service.GetBalanceAsync(userId);

        result.Should().NotBeNull();
        result.Balance.Should().Be(1000m);
        result.Currency.Should().Be("INR");
    }

    [Test]
    public async Task GetBalanceAsync_WhenWalletNotFound_ThrowsKeyNotFoundException()
    {
        _accountsMock.Setup(a => a.FindByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((WalletAccount?)null);

        Func<Task> act = async () => await _service.GetBalanceAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*not found*");
    }

    // ── TopUpAsync ────────────────────────────────────────────────────────────

    [Test]
    public async Task TopUpAsync_WhenWalletLocked_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var wallet = new WalletAccount { UserId = userId, IsLocked = true };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(wallet);

        Func<Task> act = async () => await _service.TopUpAsync(userId, new TopUpRequestDto { Amount = 100m, Provider = "Stripe" });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*locked*");
    }

    [Test]
    public async Task TopUpAsync_WhenIdempotencyKeyExists_ReturnsExistingTopUp()
    {
        var userId = Guid.NewGuid();
        var wallet = new WalletAccount { UserId = userId, SnapshotBalance = 1500m };
        var existingTopUp = new TopUpRequest { Id = Guid.NewGuid(), IdempotencyKey = "key123", Amount = 500m, Status = "Completed" };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(wallet);
        _topUpsMock.Setup(t => t.FindByIdempotencyKeyAsync("key123")).ReturnsAsync(existingTopUp);

        var result = await _service.TopUpAsync(userId, new TopUpRequestDto { Amount = 500m, IdempotencyKey = "key123", Provider = "Stripe" });

        result.Should().NotBeNull();
        result.Amount.Should().Be(500m);
        result.NewBalance.Should().Be(1500m);
        _uowMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Test]
    public async Task TopUpAsync_WhenDailyLimitExceeded_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var wallet = new WalletAccount { Id = walletId, UserId = userId, SnapshotBalance = 0 };
        var tracker = new DailyLimitTracker { TopUpTotal = 45000m };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(wallet);
        _topUpsMock.Setup(t => t.FindByIdempotencyKeyAsync(It.IsAny<string>())).ReturnsAsync((TopUpRequest?)null);
        _dailyLimitsMock.Setup(d => d.FindByWalletAndDateAsync(walletId, It.IsAny<DateTime>())).ReturnsAsync(tracker);

        var tx = new Mock<IDbContextTransaction>();
        _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(tx.Object);

        Func<Task> act = async () => await _service.TopUpAsync(userId, new TopUpRequestDto { Amount = 10000m, Provider = "Stripe" });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*exceeded*");
        tx.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task TopUpAsync_ValidRequest_ShouldProcessTopUpAndPublishEvent()
    {
        var userId = Guid.NewGuid();
        var walletId = Guid.NewGuid();
        var wallet = new WalletAccount { Id = walletId, UserId = userId, SnapshotBalance = 5000m, Currency = "INR" };
        var tracker = new DailyLimitTracker { TopUpTotal = 0 };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(wallet);
        _topUpsMock.Setup(t => t.FindByIdempotencyKeyAsync(It.IsAny<string>())).ReturnsAsync((TopUpRequest?)null);
        _dailyLimitsMock.Setup(d => d.FindByWalletAndDateAsync(walletId, It.IsAny<DateTime>())).ReturnsAsync(tracker);

        var tx = new Mock<IDbContextTransaction>();
        _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(tx.Object);

        var result = await _service.TopUpAsync(userId, new TopUpRequestDto { Amount = 1000m, Provider = "Stripe", IdempotencyKey = "key123" });

        result.Should().NotBeNull();
        result.Amount.Should().Be(1000m);
        result.NewBalance.Should().Be(6000m);
        wallet.SnapshotBalance.Should().Be(6000m);
        tracker.TopUpTotal.Should().Be(1000m);

        _topUpsMock.Verify(t => t.AddAsync(It.Is<TopUpRequest>(x => x.Amount == 1000m && x.Provider == "Stripe")), Times.Once);
        _ledgerMock.Verify(l => l.AddAsync(It.Is<LedgerEntry>(x => x.Type == "CREDIT" && x.Amount == 1000m)), Times.Once);
        tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _busMock.Verify(b => b.Publish(It.Is<TopUpCompleted>(e => e.UserId == userId && e.Amount == 1000m), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── TransferAsync ─────────────────────────────────────────────────────────

    [Test]
    public async Task TransferAsync_WhenSenderLocked_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var senderWallet = new WalletAccount { UserId = userId, IsLocked = true };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(senderWallet);

        Func<Task> act = async () => await _service.TransferAsync(userId, new TransferRequestDto { Amount = 100m, ToEmail = "recipient@example.com" }, "test-token");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*locked*");
    }

    [Test]
    public async Task TransferAsync_WhenSenderNotVerified_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var senderWallet = new WalletAccount { UserId = userId, IsLocked = false, KYCVerified = false };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(senderWallet);

        Func<Task> act = async () => await _service.TransferAsync(userId, new TransferRequestDto { Amount = 100m, ToEmail = "recipient@example.com" }, "test-token");

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*KYC*");
    }

    [Test]
    public async Task TransferAsync_WhenIdempotencyKeyExists_ReturnsExistingTransfer()
    {
        var userId = Guid.NewGuid();
        var senderWallet = new WalletAccount { UserId = userId, SnapshotBalance = 1500m, KYCVerified = true };
        var existingTransfer = new TransferRequest { Id = Guid.NewGuid(), IdempotencyKey = "t-key123", Amount = 500m, Status = "Completed" };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(senderWallet);
        _transfersMock.Setup(t => t.FindByIdempotencyKeyAsync("t-key123")).ReturnsAsync(existingTransfer);

        var result = await _service.TransferAsync(userId, new TransferRequestDto { Amount = 500m, IdempotencyKey = "t-key123", ToEmail = "recipient@example.com" }, "test-token");

        result.Should().NotBeNull();
        result.Amount.Should().Be(500m);
        result.NewBalance.Should().Be(1500m);
        _uowMock.Verify(u => u.BeginTransactionAsync(), Times.Never);
    }

    [Test]
    public async Task TransferAsync_ValidRequest_ShouldProcessTransferAndPublishEvent()
    {
        var userId = Guid.NewGuid();
        var toUserId = Guid.NewGuid();
        var senderWalletId = Guid.NewGuid();
        var receiverWalletId = Guid.NewGuid();
        const string recipientEmail = "receiver@example.com";

        var senderWallet = new WalletAccount { Id = senderWalletId, UserId = userId, SnapshotBalance = 5000m, KYCVerified = true, Currency = "INR" };
        var receiverWallet = new WalletAccount { Id = receiverWalletId, UserId = toUserId, Email = recipientEmail, SnapshotBalance = 1000m, KYCVerified = true };
        var tracker = new DailyLimitTracker { TransferTotal = 0, TransferCount = 0 };

        _accountsMock.Setup(a => a.FindByUserIdAsync(userId)).ReturnsAsync(senderWallet);
        _transfersMock.Setup(t => t.FindByIdempotencyKeyAsync(It.IsAny<string>())).ReturnsAsync((TransferRequest?)null);
        // Simulate email lookup miss (old wallet) — falls back to auth client
        _accountsMock.Setup(a => a.FindByEmailAsync(recipientEmail)).ReturnsAsync((WalletAccount?)null);
        _authClientMock.Setup(a => a.GetUserIdByEmailAsync(recipientEmail, "test-token")).ReturnsAsync(toUserId);
        _accountsMock.Setup(a => a.FindByUserIdAsync(toUserId)).ReturnsAsync(receiverWallet);
        _dailyLimitsMock.Setup(d => d.FindByWalletAndDateAsync(senderWalletId, It.IsAny<DateTime>())).ReturnsAsync(tracker);

        var tx = new Mock<IDbContextTransaction>();
        _uowMock.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(tx.Object);

        var result = await _service.TransferAsync(userId, new TransferRequestDto { Amount = 1000m, ToEmail = recipientEmail, IdempotencyKey = "t-key123" }, "test-token");

        result.Should().NotBeNull();
        result.Amount.Should().Be(1000m);
        result.NewBalance.Should().Be(4000m);
        senderWallet.SnapshotBalance.Should().Be(4000m);
        receiverWallet.SnapshotBalance.Should().Be(2000m);
        tracker.TransferTotal.Should().Be(1000m);
        tracker.TransferCount.Should().Be(1);

        _transfersMock.Verify(t => t.AddAsync(It.Is<TransferRequest>(x => x.Amount == 1000m)), Times.Once);
        _ledgerMock.Verify(l => l.AddAsync(It.Is<LedgerEntry>(x => x.Type == "DEBIT" && x.Amount == 1000m)), Times.Once);
        _ledgerMock.Verify(l => l.AddAsync(It.Is<LedgerEntry>(x => x.Type == "CREDIT" && x.Amount == 1000m)), Times.Once);
        tx.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _busMock.Verify(b => b.Publish(It.Is<TransferCompleted>(e => e.FromUserId == userId && e.ToUserId == toUserId && e.Amount == 1000m), It.IsAny<CancellationToken>()), Times.Once);
    }
}
