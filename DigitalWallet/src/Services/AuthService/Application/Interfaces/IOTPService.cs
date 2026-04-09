using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IOTPService
{
    Task SendOTPAsync(string email);
    Task<bool> VerifyOTPAsync(string email, string code);
}
