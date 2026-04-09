using System.Security.Claims;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedContracts.DTOs;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace AuthService.Tests.Controllers;

[TestFixture]
public class KYCControllerTests
{
    private Mock<IKYCService> _kycServiceMock;
    private KYCController    _controller;
    private Guid             _userId;

    [SetUp]
    public void SetUp()
    {
        _kycServiceMock = new Mock<IKYCService>();
        _userId = Guid.NewGuid();
        _controller = new KYCController(_kycServiceMock.Object);
        SetUser(_controller, _userId);
    }

    // ── Submit ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Submit_ValidRequest_ReturnsOkWithKYCStatusResponse()
    {
        var request  = new KYCSubmitRequest { DocType = "Passport", FileUrl = "https://cdn.example.com/doc.pdf" };
        var expected = new KYCStatusResponse { DocumentId = Guid.NewGuid(), DocType = "Passport", Status = "Pending", SubmittedAt = DateTime.UtcNow };
        _kycServiceMock.Setup(s => s.SubmitAsync(_userId, request)).ReturnsAsync(expected);

        var result = await _controller.Submit(request);

        var ok       = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<KYCStatusResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.DocType.Should().Be("Passport");
        response.Data.Status.Should().Be("Pending");
        _kycServiceMock.Verify(s => s.SubmitAsync(_userId, request), Times.Once);
    }

    [Test]
    public async Task Submit_AlreadyPending_ExceptionPropagates()
    {
        var request = new KYCSubmitRequest { DocType = "Passport", FileUrl = "https://cdn.example.com/doc.pdf" };
        _kycServiceMock.Setup(s => s.SubmitAsync(_userId, request))
                       .ThrowsAsync(new InvalidOperationException("A Passport document is already pending review."));

        var act = () => _controller.Submit(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*pending review*");
    }

    [Test]
    public async Task Submit_UserNotFound_ExceptionPropagates()
    {
        var request = new KYCSubmitRequest { DocType = "NationalId", FileUrl = "https://cdn.example.com/id.pdf" };
        _kycServiceMock.Setup(s => s.SubmitAsync(_userId, request))
                       .ThrowsAsync(new InvalidOperationException("User not found."));

        var act = () => _controller.Submit(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── GetStatus ─────────────────────────────────────────────────────────────

    [Test]
    public async Task GetStatus_UserWithDocuments_ReturnsOkWithList()
    {
        var expected = new List<KYCStatusResponse>
        {
            new() { DocumentId = Guid.NewGuid(), DocType = "Passport",   Status = "Approved", SubmittedAt = DateTime.UtcNow.AddDays(-5) },
            new() { DocumentId = Guid.NewGuid(), DocType = "NationalId", Status = "Pending",  SubmittedAt = DateTime.UtcNow.AddDays(-1) }
        };
        _kycServiceMock.Setup(s => s.GetStatusAsync(_userId)).ReturnsAsync(expected);

        var result = await _controller.GetStatus();

        var ok       = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<KYCStatusResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().HaveCount(2);
        response.Data![0].DocType.Should().Be("Passport");
    }

    [Test]
    public async Task GetStatus_NoDocuments_ReturnsOkWithEmptyList()
    {
        _kycServiceMock.Setup(s => s.GetStatusAsync(_userId)).ReturnsAsync(new List<KYCStatusResponse>());

        var result = await _controller.GetStatus();

        var ok       = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<KYCStatusResponse>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEmpty();
    }

    [Test]
    public async Task GetStatus_UsesCorrectUserIdFromClaims()
    {
        _kycServiceMock.Setup(s => s.GetStatusAsync(_userId)).ReturnsAsync(new List<KYCStatusResponse>());

        await _controller.GetStatus();

        _kycServiceMock.Verify(s => s.GetStatusAsync(_userId), Times.Once);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void SetUser(ControllerBase controller, Guid userId)
    {
        var claims    = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
    }
}
