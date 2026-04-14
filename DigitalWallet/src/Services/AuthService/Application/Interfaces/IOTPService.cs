using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

/// <summary>
/// Defines the contract for sending and verifying one-time passwords.
/// </summary>
public interface IOTPService
{
    /// <summary>
    /// Generates and delivers a new OTP to the specified email address.
    /// </summary>
    Task SendOTPAsync(string email);
    /// <summary>
    /// Validates the provided OTP code for the given email and marks it as used on success.
    /// </summary>
    Task<bool> VerifyOTPAsync(string email, string code);
}
