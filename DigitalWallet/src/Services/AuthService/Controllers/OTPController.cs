using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedContracts.DTOs;

namespace AuthService.Controllers;

[ApiController]
[Route("api/auth/otp")]
public class OTPController : ControllerBase
{
    private readonly IOTPService _otpService;

    public OTPController(IOTPService otpService)
    {
        _otpService = otpService;
    }

    /// <summary>
    /// Send an OTP to the user's email.
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] OTPSendRequest request)
    {
        await _otpService.SendOTPAsync(request.Email);
        return Ok(ApiResponse<object>.Ok(new { Message = "OTP sent successfully." }));
    }

    /// <summary>
    /// Verify an OTP code.
    /// </summary>
    [HttpPost("verify")]
    public async Task<IActionResult> Verify([FromBody] OTPVerifyRequest request)
    {
        var verified = await _otpService.VerifyOTPAsync(request.Email, request.Code);
        if (!verified)
            return BadRequest(ApiResponse<string>.Fail("Invalid or expired OTP."));

        return Ok(ApiResponse<string>.Ok("Verified", "OTP verified successfully."));
    }
}
