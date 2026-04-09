using AuthService.Application.Interfaces.Repositories;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AuthDbContext _db;
    public UserRepository(AuthDbContext db) => _db = db;

    public Task<bool> ExistsAsync(string email, string phone) => _db.Users.AnyAsync(u => u.Email == email || u.Phone == phone);

    public Task<User?> FindByEmailAsync(string email) =>_db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> FindByIdAsync(Guid id) =>_db.Users.FindAsync(id).AsTask();

    public Task<List<User>> GetAllAsync() =>
        _db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(User user) =>await _db.Users.AddAsync(user);

    public Task SaveAsync() => _db.SaveChangesAsync();
}
