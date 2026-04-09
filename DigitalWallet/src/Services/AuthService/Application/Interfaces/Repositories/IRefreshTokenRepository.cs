using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindActiveByTokenAsync(string token);
    Task<RefreshToken?> FindActiveByUserAndTokenAsync(Guid userId, string token);
    Task AddAsync(RefreshToken token);
    Task SaveAsync();
}
