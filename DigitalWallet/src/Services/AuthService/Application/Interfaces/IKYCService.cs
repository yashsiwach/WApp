using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

public interface IKYCService
{
    Task<KYCStatusResponse> SubmitAsync(Guid userId, KYCSubmitRequest request);
    Task<List<KYCStatusResponse>> GetStatusAsync(Guid userId);
}
