using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces.Repositories;

public interface IOTPRepository
{
    Task AddAsync(OTPLog log);
    Task<OTPLog?> FindValidAsync(string email, string code);
    Task<bool> HasRecentlyVerifiedOtpAsync(string email, TimeSpan maxAge);
    Task MarkAsUsedAsync(Guid id);
    Task SaveAsync();
}
