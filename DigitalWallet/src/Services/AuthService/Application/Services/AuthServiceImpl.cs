using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Application.Interfaces.Repositories;
using AuthService.Application.Mappers;
using AuthService.Domain.Entities;
using MassTransit;
using SharedContracts.Events;

namespace AuthService.Application.Services;

public class AuthServiceImpl : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IOTPRepository _otps;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly ITokenService _tokenService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AuthServiceImpl> _logger;

    /// <summary>
    /// Initializes authentication dependencies for user, token, and event publishing operations.
    /// </summary>
    public AuthServiceImpl(
        IUserRepository users,
        IOTPRepository otps,
        IRefreshTokenRepository refreshTokens,
        ITokenService tokenService,
        IPublishEndpoint publishEndpoint,
        ILogger<AuthServiceImpl> logger)
    {
        _users           = users;
        _otps            = otps;
        _refreshTokens   = refreshTokens;
        _tokenService    = tokenService;
        _publishEndpoint = publishEndpoint;
        _logger          = logger;
    }

    /// <summary>
    /// Registers a new user, stores credentials, publishes registration event, and returns access and refresh tokens.
    /// </summary>
    public async Task<AuthResponse> SignupAsync(SignupRequest request)
    {
        var exists = await _users.ExistsAsync(request.Email, request.Phone);
        if (exists)
            throw new InvalidOperationException("User with this email or phone already exists.");

        var hasVerifiedOtp = await _otps.HasRecentlyVerifiedOtpAsync(request.Email, TimeSpan.FromMinutes(15));
        if (!hasVerifiedOtp)
            throw new InvalidOperationException("Verify your email with OTP before registering.");

        var user = new User
        {
            Email        = request.Email,
            Phone        = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role         = "User",
            IsActive     = true
        };

        await _users.AddAsync(user);
        await _users.SaveAsync();

        _logger.LogInformation("User registered: {UserId} {Email}", user.Id, user.Email);

        // Fire-and-forget: user is already saved, wallet/rewards creation is resilient via lazy provisioning
        _ = _publishEndpoint.Publish(new UserRegistered
        {
            UserId     = user.Id,
            Email      = user.Email,
            Phone      = user.Phone,
            Role       = user.Role,
            OccurredAt = DateTime.UtcNow
        }).ContinueWith(t => _logger.LogError(t.Exception, "Failed to publish UserRegistered for {UserId}", user.Id),
            TaskContinuationOptions.OnlyOnFaulted);

        // Issue tokens immediately so the client is authenticated right after signup.
        
        return await IssueTokenPairAsync(user);//EOD
    }

    /// <summary>
    /// Validates user credentials and active status, then issues a fresh access and refresh token pair.
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is inactive. Verify OTP to activate your account.");

        _logger.LogInformation("User logged in: {UserId}", user.Id);

        return await IssueTokenPairAsync(user);//EOD
    }

    /// <summary>
    /// Revokes the provided refresh token if valid and returns a newly issued access and refresh token pair.
    /// </summary>
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var stored = await _refreshTokens.FindActiveByTokenAsync(refreshToken);
        if (stored == null || stored.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        stored.Revoked = true;
        await _refreshTokens.SaveAsync();

        return await IssueTokenPairAsync(stored.User);
    }

    /// <summary>
    /// Revokes the specified active refresh token for the user to complete logout.
    /// </summary>
    public async Task LogoutAsync(Guid userId, string refreshToken)
    {
        var token = await _refreshTokens.FindActiveByUserAndTokenAsync(userId, refreshToken);
        if (token != null)
        {
            token.Revoked = true;
            await _refreshTokens.SaveAsync();
        }

        _logger.LogInformation("User logged out: {UserId}", userId);
    }

    /// <summary>
    /// Resolves a user's ID from their email address, used by other services for transfers.
    /// </summary>
    public async Task<Guid?> GetUserIdByEmailAsync(string email)
    {
        var user = await _users.FindByEmailAsync(email.ToLower());
        return user?.Id;
    }

    /// <summary>
    /// Returns all users for admin management views.
    /// </summary>
    public async Task<List<UserDto>> GetUsersAsync()
    {
        var users = await _users.GetAllAsync();
        return users.Select(AuthMapper.ToDto).ToList();
    }

    /// <summary>
    /// Updates active status (block/unblock) for a user from admin panel.
    /// </summary>
    public async Task<UserDto> SetUserStatusAsync(Guid userId, bool isActive)
    {
        var user = await _users.FindByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _users.SaveAsync();

        _logger.LogInformation("User status changed: {UserId} -> IsActive={IsActive}", userId, isActive);
        return AuthMapper.ToDto(user);
    }

    // ──────── Private helpers ────────

    /// <summary>Generates access and refresh tokens, persists the refresh token, and maps the auth response payload.</summary>
    private async Task<AuthResponse> IssueTokenPairAsync(User user)
    {
        var (accessToken, expiresAt) = _tokenService.GenerateAccessToken(user);//service in Infrastructure layer

        var refreshToken = new RefreshToken
        {
            UserId    = user.Id,
            Token     = _tokenService.GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _refreshTokens.AddAsync(refreshToken);
        await _refreshTokens.SaveAsync();

        return AuthMapper.ToAuthResponse(accessToken, refreshToken.Token, expiresAt, user);
    }
}
