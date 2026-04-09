using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Application.Interfaces.Repositories;
using NotificationService.Application.Services;
using NotificationService.Domain.Entities;
using NUnit.Framework;
using SharedContracts.DTOs;

namespace NotificationService.Tests;

[TestFixture]
public class NotificationServiceTests
{
    private Mock<INotificationLogRepository>      _logsMock;
    private Mock<INotificationTemplateRepository> _templatesMock;
    private Mock<IEmailSender>                    _emailSenderMock;
    private Mock<ILogger<NotificationServiceImpl>> _loggerMock;
    private NotificationServiceImpl               _service;

    [SetUp]
    public void SetUp()
    {
        _logsMock        = new Mock<INotificationLogRepository>();
        _templatesMock   = new Mock<INotificationTemplateRepository>();
        _emailSenderMock = new Mock<IEmailSender>();
        _loggerMock      = new Mock<ILogger<NotificationServiceImpl>>();

        _service = new NotificationServiceImpl(
            _logsMock.Object,
            _templatesMock.Object,
            _emailSenderMock.Object,
            _loggerMock.Object);
    }

    // ── SendAsync ─────────────────────────────────────────────────────────────

    [Test]
    public async Task SendAsync_EmailChannel_ShouldPersistLogAndSendEmail()
    {
        var userId    = Guid.NewGuid();
        var template  = new NotificationTemplate { Subject = "Hello {{Name}}", BodyTemplate = "Dear {{Name}}", Type = "Welcome", Channel = "Email" };
        var placeholders = new Dictionary<string, string> { ["Name"] = "Alice" };

        _templatesMock.Setup(t => t.FindByTypeAndChannelAsync("Welcome", "Email")).ReturnsAsync(template);

        await _service.SendAsync(userId, "Email", "Welcome", "alice@test.com", placeholders);

        _logsMock.Verify(l => l.AddAsync(It.Is<NotificationLog>(n =>
            n.UserId    == userId &&
            n.Channel   == "Email" &&
            n.Type      == "Welcome" &&
            n.Recipient == "alice@test.com")), Times.Once);

        _emailSenderMock.Verify(e => e.SendAsync("alice@test.com", "Alice", "Hello Alice", "Dear Alice"), Times.Once);

        _logsMock.Verify(l => l.SaveAsync(), Times.Exactly(2));
    }

    [Test]
    public async Task SendAsync_EmailSenderThrows_ShouldMarkLogAsFailed()
    {
        var userId = Guid.NewGuid();
        _templatesMock.Setup(t => t.FindByTypeAndChannelAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync((NotificationTemplate?)null);
        _emailSenderMock.Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                        .ThrowsAsync(new Exception("SMTP error"));

        NotificationLog? capturedLog = null;
        _logsMock.Setup(l => l.AddAsync(It.IsAny<NotificationLog>()))
                 .Callback<NotificationLog>(log => capturedLog = log);

        await _service.SendAsync(userId, "Email", "TopUp", "user@test.com", new Dictionary<string, string>());

        capturedLog.Should().NotBeNull();
        capturedLog!.Status.Should().Be("Failed");
        capturedLog.ErrorMessage.Should().Be("SMTP error");
    }

    [Test]
    public async Task SendAsync_UnknownChannel_ShouldNotCallEmailSender()
    {
        var userId = Guid.NewGuid();
        _templatesMock.Setup(t => t.FindByTypeAndChannelAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync((NotificationTemplate?)null);

        await _service.SendAsync(userId, "SMS", "TopUp", "+911234567890", new Dictionary<string, string>());

        _emailSenderMock.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task SendAsync_NoTemplate_ShouldUseFallbackSubjectAndBody()
    {
        var userId = Guid.NewGuid();
        _templatesMock.Setup(t => t.FindByTypeAndChannelAsync(It.IsAny<string>(), It.IsAny<string>()))
                      .ReturnsAsync((NotificationTemplate?)null);

        NotificationLog? capturedLog = null;
        _logsMock.Setup(l => l.AddAsync(It.IsAny<NotificationLog>()))
                 .Callback<NotificationLog>(log => capturedLog = log);

        await _service.SendAsync(userId, "Email", "TopUp", "user@test.com", new Dictionary<string, string>());

        capturedLog!.Subject.Should().Contain("TopUp");
    }

    // ── GetLogsAsync ──────────────────────────────────────────────────────────

    [Test]
    public async Task GetLogsAsync_ShouldDelegateToRepository()
    {
        var userId = Guid.NewGuid();
        var expected = new PaginatedResult<NotificationLogDto>
        {
            Items      = new List<NotificationLogDto> { new() { Id = Guid.NewGuid(), Type = "Welcome", Status = "Sent" } },
            Page       = 1,
            PageSize   = 10,
            TotalCount = 1
        };
        _logsMock.Setup(l => l.GetPagedByUserIdAsync(userId, 1, 10)).ReturnsAsync(expected);

        var result = await _service.GetLogsAsync(userId, 1, 10);

        result.Should().BeEquivalentTo(expected);
        _logsMock.Verify(l => l.GetPagedByUserIdAsync(userId, 1, 10), Times.Once);
    }

    // ── GetTemplatesAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task GetTemplatesAsync_ShouldReturnMappedDtos()
    {
        var templates = new List<NotificationTemplate>
        {
            new() { Id = Guid.NewGuid(), Type = "Welcome", Channel = "Email", Subject = "Hi", BodyTemplate = "Body", IsActive = true }
        };
        _templatesMock.Setup(t => t.GetAllAsync()).ReturnsAsync(templates);

        var result = await _service.GetTemplatesAsync();

        result.Should().HaveCount(1);
        result[0].Type.Should().Be("Welcome");
        result[0].Channel.Should().Be("Email");
        result[0].IsActive.Should().BeTrue();
    }

    // ── UpdateTemplateAsync ───────────────────────────────────────────────────

    [Test]
    public async Task UpdateTemplateAsync_WhenTemplateExists_ShouldUpdateAndReturn()
    {
        var templateId = Guid.NewGuid();
        var template   = new NotificationTemplate { Id = templateId, Type = "TopUp", Channel = "Email", Subject = "Old Subject", BodyTemplate = "Old Body", IsActive = true };
        var dto        = new UpdateTemplateDto { Subject = "New Subject", BodyTemplate = "New Body", IsActive = false };

        _templatesMock.Setup(t => t.FindByIdAsync(templateId)).ReturnsAsync(template);

        var result = await _service.UpdateTemplateAsync(templateId, dto);

        result.Subject.Should().Be("New Subject");
        result.BodyTemplate.Should().Be("New Body");
        result.IsActive.Should().BeFalse();
        _templatesMock.Verify(t => t.SaveAsync(), Times.Once);
    }

    [Test]
    public async Task UpdateTemplateAsync_WhenTemplateNotFound_ShouldThrowKeyNotFoundException()
    {
        _templatesMock.Setup(t => t.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((NotificationTemplate?)null);
        var dto = new UpdateTemplateDto { Subject = "X", BodyTemplate = "Y", IsActive = true };

        Func<Task> act = async () => await _service.UpdateTemplateAsync(Guid.NewGuid(), dto);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Template not found.");
    }
}
