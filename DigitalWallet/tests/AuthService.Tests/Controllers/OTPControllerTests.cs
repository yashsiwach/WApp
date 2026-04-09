using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SharedContracts.DTOs;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace AuthService.Tests.Controllers;

[TestFixture]
public class OTPControllerTests
{
    private Mock<IOTPService> _otpServiceMock;
    private OTPController    _controller;

    [SetUp]
    public void SetUp()
    {
        _otpServiceMock = new Mock<IOTPService>();
        _controller = new OTPController(_otpServiceMock.Object);
    }

    // ── Send ──────────────────────────────────────────────────────────────────

    [Test]
    public async Task Send_ValidEmail_ReturnsOkWithCode()
    {
        var request = new OTPSendRequest { Email = "user@test.com" };
        _otpServiceMock.Setup(s => s.SendOTPAsync("user@test.com")).Returns(Task.CompletedTask);

        var result = await _controller.Send(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
        _otpServiceMock.Verify(s => s.SendOTPAsync("user@test.com"), Times.Once);
    }

    [Test]
    public async Task Send_UserNotFound_ExceptionPropagates()
    {
        var request = new OTPSendRequest { Email = "unknown@test.com" };
        _otpServiceMock.Setup(s => s.SendOTPAsync("unknown@test.com"))
                       .ThrowsAsync(new KeyNotFoundException("User not found."));

        var act = () => _controller.Send(request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Test]
    public async Task Send_CallsServiceWithCorrectEmail()
    {
        var email   = "specific@test.com";
        var request = new OTPSendRequest { Email = email };
        _otpServiceMock.Setup(s => s.SendOTPAsync(email)).Returns(Task.CompletedTask);

        await _controller.Send(request);

        _otpServiceMock.Verify(s => s.SendOTPAsync(email), Times.Once);
    }

    // ── Verify ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Verify_ValidCode_ReturnsOk()
    {
        var request = new OTPVerifyRequest { Email = "user@test.com", Code = "123456" };
        _otpServiceMock.Setup(s => s.VerifyOTPAsync("user@test.com", "123456")).ReturnsAsync(true);

        var result = await _controller.Verify(request);

        var ok       = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Contain("verified");
    }

    [Test]
    public async Task Verify_InvalidCode_ReturnsBadRequest()
    {
        var request = new OTPVerifyRequest { Email = "user@test.com", Code = "000000" };
        _otpServiceMock.Setup(s => s.VerifyOTPAsync("user@test.com", "000000")).ReturnsAsync(false);

        var result = await _controller.Verify(request);

        var bad      = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var response = bad.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        response.Success.Should().BeFalse();
        response.Message.Should().Contain("Invalid");
    }

    [Test]
    public async Task Verify_ExpiredCode_ReturnsBadRequest()
    {
        var request = new OTPVerifyRequest { Email = "user@test.com", Code = "111111" };
        _otpServiceMock.Setup(s => s.VerifyOTPAsync("user@test.com", "111111")).ReturnsAsync(false);

        var result = await _controller.Verify(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task Verify_CallsServiceWithCorrectArguments()
    {
        var email   = "user@test.com";
        var code    = "999888";
        var request = new OTPVerifyRequest { Email = email, Code = code };
        _otpServiceMock.Setup(s => s.VerifyOTPAsync(email, code)).ReturnsAsync(true);

        await _controller.Verify(request);

        _otpServiceMock.Verify(s => s.VerifyOTPAsync(email, code), Times.Once);
    }
}
