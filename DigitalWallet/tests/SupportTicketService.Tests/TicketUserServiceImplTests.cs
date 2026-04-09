using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SharedContracts.DTOs;
using SharedContracts.Events;
using SupportTicketService.Application.DTOs;
using SupportTicketService.Application.Interfaces.Repositories;
using SupportTicketService.Application.Services;
using SupportTicketService.Domain.Entities;

namespace SupportTicketService.Tests;

[TestFixture]
public class TicketUserServiceImplTests
{
    private Mock<IUnitOfWork> _uowMock;
    private Mock<ISupportTicketRepository> _ticketsMock;
    private Mock<ITicketReplyRepository> _repliesMock;
    private Mock<IPublishEndpoint> _busMock;
    private Mock<ILogger<TicketUserServiceImpl>> _loggerMock;
    private TicketUserServiceImpl _service;

    [SetUp]
    public void SetUp()
    {
        _ticketsMock = new Mock<ISupportTicketRepository>();
        _repliesMock = new Mock<ITicketReplyRepository>();
        _busMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<TicketUserServiceImpl>>();

        _uowMock = new Mock<IUnitOfWork>();
        _uowMock.Setup(u => u.Tickets).Returns(_ticketsMock.Object);
        _uowMock.Setup(u => u.Replies).Returns(_repliesMock.Object);

        _service = new TicketUserServiceImpl(_uowMock.Object, _busMock.Object, _loggerMock.Object);
    }

    // ── CreateAsync ───────────────────────────────────────────────────────────

    [Test]
    public async Task CreateAsync_ValidRequest_ShouldCreateTicketAndPublishEvent()
    {
        var userId = Guid.NewGuid();
        var request = new CreateTicketRequest { Subject = "Help", Description = "Need help", Priority = "High", Category = "General" };

        var result = await _service.CreateAsync(userId, "user@test.com", request);

        result.Should().NotBeNull();
        result.Subject.Should().Be("Help");
        result.TicketNumber.Should().StartWith("TKT-");
        
        _ticketsMock.Verify(t => t.AddAsync(It.Is<SupportTicket>(x => x.Subject == "Help" && x.UserId == userId)), Times.Once);
        _uowMock.Verify(u => u.SaveAsync(), Times.Once);
        _busMock.Verify(b => b.Publish(It.Is<TicketCreated>(e => e.UserId == userId && e.Subject == "Help"), It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── GetMyTicketsAsync ─────────────────────────────────────────────────────

    [Test]
    public async Task GetMyTicketsAsync_ShouldDelegateToRepository()
    {
        var userId = Guid.NewGuid();
        var expectedResult = new PaginatedResult<TicketSummaryDto>
        {
            Items = new List<TicketSummaryDto> { new() { Id = Guid.NewGuid(), Subject = "Test" } },
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        _ticketsMock.Setup(t => t.GetByUserIdPagedAsync(userId, 1, 10, null)).ReturnsAsync(expectedResult);

        var result = await _service.GetMyTicketsAsync(userId, 1, 10, null);

        result.Should().BeEquivalentTo(expectedResult);
        _ticketsMock.Verify(t => t.GetByUserIdPagedAsync(userId, 1, 10, null), Times.Once);
    }

    // ── GetMyTicketByIdAsync ──────────────────────────────────────────────────

    [Test]
    public async Task GetMyTicketByIdAsync_WhenFoundAndOwned_ReturnsDto()
    {
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var ticket = new SupportTicket { Id = ticketId, UserId = userId, Subject = "Hello", TicketNumber = "TKT-123", Status = "Open" };

        _ticketsMock.Setup(t => t.FindByIdWithRepliesAsync(ticketId)).ReturnsAsync(ticket);

        var result = await _service.GetMyTicketByIdAsync(userId, ticketId);

        result.Should().NotBeNull();
        result.Id.Should().Be(ticketId);
    }

    [Test]
    public async Task GetMyTicketByIdAsync_WhenNotOwned_ThrowsUnauthorizedAccessException()
    {
        var ticketId = Guid.NewGuid();
        var ticket = new SupportTicket { Id = ticketId, UserId = Guid.NewGuid() }; // Different user id

        _ticketsMock.Setup(t => t.FindByIdWithRepliesAsync(ticketId)).ReturnsAsync(ticket);

        Func<Task> act = async () => await _service.GetMyTicketByIdAsync(Guid.NewGuid(), ticketId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*access*");
    }

    [Test]
    public async Task GetMyTicketByIdAsync_WhenNotFound_ThrowsKeyNotFoundException()
    {
        _ticketsMock.Setup(t => t.FindByIdWithRepliesAsync(It.IsAny<Guid>())).ReturnsAsync((SupportTicket?)null);

        Func<Task> act = async () => await _service.GetMyTicketByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*not found*");
    }

    // ── AddReplyAsync ─────────────────────────────────────────────────────────

    [Test]
    public async Task AddReplyAsync_WhenTicketClosed_ThrowsInvalidOperationException()
    {
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var ticket = new SupportTicket { Id = ticketId, UserId = userId, Status = "Closed" };

        _ticketsMock.Setup(t => t.FindByIdWithRepliesAsync(ticketId)).ReturnsAsync(ticket);

        Func<Task> act = async () => await _service.AddReplyAsync(userId, ticketId, new AddReplyRequest { Message = "Reply" });

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*closed ticket*");
    }

    [Test]
    public async Task AddReplyAsync_ValidRequest_ShouldAddReplyAndPublishEvent()
    {
        var userId = Guid.NewGuid();
        var ticketId = Guid.NewGuid();
        var ticket = new SupportTicket { Id = ticketId, UserId = userId, Status = "Resolved", TicketNumber = "TKT-1", Subject = "Bug" };

        _ticketsMock.Setup(t => t.FindByIdWithRepliesAsync(ticketId)).ReturnsAsync(ticket);

        var request = new AddReplyRequest { Message = "Still broken" };
        var result = await _service.AddReplyAsync(userId, ticketId, request);

        ticket.Status.Should().Be("Open"); // Verify it gets reopened
        _repliesMock.Verify(r => r.AddAsync(It.Is<TicketReply>(tr => tr.Message == "Still broken" && tr.AuthorId == userId)), Times.Once);
        _uowMock.Verify(u => u.SaveAsync(), Times.Once);
        _busMock.Verify(b => b.Publish(It.Is<TicketReplied>(e => e.TicketId == ticketId && e.ReplyMessage == "Still broken"), It.IsAny<CancellationToken>()), Times.Once);

        result.Should().NotBeNull();
        result.Message.Should().Be("Still broken");
    }
}
