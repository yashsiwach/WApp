using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces;

/// <summary>
/// Defines the contract for KYC document submission and status retrieval.
/// </summary>
public interface IKYCService
{
    /// <summary>
    /// Submits a new KYC document on behalf of the specified user.
    /// </summary>
    Task<KYCStatusResponse> SubmitAsync(Guid userId, KYCSubmitRequest request);
    /// <summary>
    /// Returns all KYC document submissions and their current review statuses for a user.
    /// </summary>
    Task<List<KYCStatusResponse>> GetStatusAsync(Guid userId);
}
