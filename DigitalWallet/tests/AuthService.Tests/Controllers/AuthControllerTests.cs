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
public class AuthControllerTests
{
    private Mock<IAuthService> _authServiceMock;
    private AuthController     _controller;
    private Guid               _userId;

    [SetUp]
    public void SetUp()
    {
        _authServiceMock = new Mock<IAuthService>();
        _userId = Guid.NewGuid();
        _controller = new AuthController(_authServiceMock.Object);
        SetUser(_controller, _userId);
    }

    // ── Signup ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Signup_ValidRequest_ReturnsOkWithAuthResponse()
    {
        var request  = new SignupRequest { Email = "user@test.com", Phone = "9999999999", Password = "Pass@123" };
        var expected = BuildAuthResponse();
        _authServiceMock.Setup(s => s.SignupAsync(request)).ReturnsAsync(expected);

        var result = await _controller.Signup(request);

        var ok       = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expected);
        _authServiceMock.Verify(s => s.SignupAsync(request), Times.Once);
    }

    [Test]
    public async Task Signup_ServiceThrowsInvalidOperation_ExceptionPropagates()
    {
        var request = new SignupRequest { Email = "dup@test.com", Phone = "9999999999", Password = "Pass@123" };
        _authServiceMock.Setup(s => s.SignupAsync(request))
                        .ThrowsAsync(new InvalidOperationException("User with this email or phone already exists."));

        var act = () => _controller.Signup(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*already exists*");
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Test]
    public async Task Login_ValidCredentials_ReturnsOkWithAuthResponse()
    {
        var request  = new LoginRequest { Email = "user@test.com", Password = "Pass@123" };
        var expected = BuildAuthResponse();
        _authServiceMock.Setup(s => s.LoginAsync(request)).ReturnsAsync(expected);

        var result = await _controller.Login(request);

        var ok       = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.AccessToken.Should().Be(expected.AccessToken);
        _authServiceMock.Verify(s => s.LoginAsync(request), Times.Once);
    }

    [Test]
    public async Task Login_InvalidCredentials_ExceptionPropagates()
    {
        var request = new LoginRequest { Email = "bad@test.com", Password = "wrong" };
        _authServiceMock.Setup(s => s.LoginAsync(request))
                        .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password."));

        var act = () => _controller.Login(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Test]
    public async Task Login_DeactivatedAccount_ExceptionPropagates()
    {
        var request = new LoginRequest { Email = "banned@test.com", Password = "Pass@123" };
        _authServiceMock.Setup(s => s.LoginAsync(request))
                        .ThrowsAsync(new UnauthorizedAccessException("Account is deactivated."));

        var act = () => _controller.Login(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("*deactivated*");
    }

    // ── RefreshToken ──────────────────────────────────────────────────────────

    [Test]
    public async Task RefreshToken_ValidToken_ReturnsOkWithNewAuthResponse()
    {
        var refreshToken = Guid.NewGuid().ToString();
        var request      = new RefreshTokenRequest { RefreshToken = refreshToken };
        var expected     = BuildAuthResponse();
        _authServiceMock.Setup(s => s.RefreshTokenAsync(refreshToken)).ReturnsAsync(expected);

        var result = await _controller.RefreshToken(request);

        var ok       = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<AuthResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.RefreshToken.Should().Be(expected.RefreshToken);
        _authServiceMock.Verify(s => s.RefreshTokenAsync(refreshToken), Times.Once);
    }

    [Test]
    public async Task RefreshToken_ExpiredToken_ExceptionPropagates()
    {
        var request = new RefreshTokenRequest { RefreshToken = "expired-token" };
        _authServiceMock.Setup(s => s.RefreshTokenAsync("expired-token"))
                        .ThrowsAsync(new UnauthorizedAccessException("Invalid or expired refresh token."));

        var act = () => _controller.RefreshToken(request);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [Test]
    public async Task Logout_AuthenticatedUser_ReturnsOk()
    {
        var token   = Guid.NewGuid().ToString();
        var request = new RefreshTokenRequest { RefreshToken = token };
        _authServiceMock.Setup(s => s.LogoutAsync(_userId, token)).Returns(Task.CompletedTask);

        var result = await _controller.Logout(request);

        var ok       = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<string>>().Subject;
        response.Success.Should().BeTrue();
        _authServiceMock.Verify(s => s.LogoutAsync(_userId, token), Times.Once);
    }

    [Test]
    public async Task Logout_CallsServiceWithCorrectUserId()
    {
        var token   = "some-refresh-token";
        var request = new RefreshTokenRequest { RefreshToken = token };

        await _controller.Logout(request);

        _authServiceMock.Verify(s => s.LogoutAsync(_userId, token), Times.Once);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static AuthResponse BuildAuthResponse() => new()
    {
        AccessToken  = "eyJhbGciOiJIUzI1NiJ9.test",
        RefreshToken = Guid.NewGuid().ToString(),
        ExpiresAt    = DateTime.UtcNow.AddMinutes(60),
        User         = new UserDto { Id = Guid.NewGuid(), Email = "user@test.com", Phone = "9999999999", Role = "User", IsActive = true, CreatedAt = DateTime.UtcNow }
    };

    private static void SetUser(ControllerBase controller, Guid userId, string role = "User")
    {
        var claims    = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Role, role) };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
    }
}
