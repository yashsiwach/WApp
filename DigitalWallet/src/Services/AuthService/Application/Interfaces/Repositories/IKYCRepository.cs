using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces.Repositories;

public interface IKYCRepository
{
    Task<bool> HasPendingAsync(Guid userId, string docType);
    Task<User?> FindUserByIdAsync(Guid userId);
    Task AddAsync(KYCDocument doc);
    Task<List<KYCDocument>> GetByUserIdAsync(Guid userId);
    Task SaveAsync();
}
