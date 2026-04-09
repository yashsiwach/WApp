using Microsoft.EntityFrameworkCore;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Domain.Entities;
using RewardsService.Infrastructure.Data;

namespace RewardsService.Infrastructure.Repositories;

public class EarnRuleRepository : IEarnRuleRepository
{
    private readonly RewardsDbContext _db;
    public EarnRuleRepository(RewardsDbContext db) => _db = db;

    public Task<EarnRule?> FindActiveByTriggerAsync(string triggerType) =>
        _db.EarnRules.Where(r => r.TriggerType == triggerType && r.IsActive).FirstOrDefaultAsync();
}
