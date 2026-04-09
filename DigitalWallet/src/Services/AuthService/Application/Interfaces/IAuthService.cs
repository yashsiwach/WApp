using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> SignupAsync(SignupRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(Guid userId, string refreshToken);
    Task<List<UserDto>> GetUsersAsync();
    Task<UserDto> SetUserStatusAsync(Guid userId, bool isActive);
    Task<Guid?> GetUserIdByEmailAsync(string email);
}
