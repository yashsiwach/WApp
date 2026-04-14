using Microsoft.EntityFrameworkCore;
using RewardsService.Application.Interfaces.Repositories;
using RewardsService.Domain.Entities;
using RewardsService.Infrastructure.Data;

namespace RewardsService.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IEarnRuleRepository for querying active earn rules.
/// </summary>
public class EarnRuleRepository : IEarnRuleRepository
{
    private readonly RewardsDbContext _db;
    /// <summary>
    /// Initializes the repository with the rewards database context.
    /// </summary>
    public EarnRuleRepository(RewardsDbContext db) => _db = db;

    /// <summary>
    /// Returns the first active earn rule matching the trigger type, or null if none found.
    /// </summary>
    public Task<EarnRule?> FindActiveByTriggerAsync(string triggerType) =>
        _db.EarnRules.Where(r => r.TriggerType == triggerType && r.IsActive).FirstOrDefaultAsync();
}
