using System.Security.Claims;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedContracts.DTOs;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request)
    {
        var result = await _authService.SignupAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "User registered successfully."));
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
    }

    /// <summary>
    /// Refresh an expired access token.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Token refreshed."));
    }

    /// <summary>
    /// List all users for admin panel.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/users")]
    public async Task<IActionResult> GetUsers()
    {
        var result = await _authService.GetUsersAsync();
        return Ok(ApiResponse<List<UserDto>>.Ok(result));
    }

    /// <summary>
    /// Block or unblock a user from admin panel.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("admin/users/{userId:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusRequest request)
    {
        var result = await _authService.SetUserStatusAsync(userId, request.IsActive);
        return Ok(ApiResponse<UserDto>.Ok(result, request.IsActive ? "User unblocked." : "User blocked."));
    }

    /// <summary>
    /// Look up a user's ID by email — used internally by other services for transfers.
    /// </summary>
    [Authorize]
    [HttpGet("users/lookup")]
    public async Task<IActionResult> LookupUserByEmail([FromQuery] string email)
    {
        var userId = await _authService.GetUserIdByEmailAsync(email);
        if (userId == null)
            return NotFound(ApiResponse<object>.Fail("No user found with that email."));
        return Ok(ApiResponse<object>.Ok(new { userId }));
    }

    /// <summary>
    /// Logout and revoke refresh token.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)?? User.FindFirstValue("sub")!);
        await _authService.LogoutAsync(userId, request.RefreshToken);
        return Ok(ApiResponse<string>.Ok("Logged out", "Logout successful."));
    }
}
