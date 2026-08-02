using eProrab.Application.Interfaces;
using eProrab.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Infrastructure.Services;

public class UserDirectoryService(UserManager<ApplicationUser> userManager) : IUserDirectoryService
{
    public async Task<UserSummary?> GetSummaryAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user is null ? null : Map(user);
    }

    public async Task<IReadOnlyDictionary<Guid, UserSummary>> GetSummariesAsync(IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, UserSummary>();
        }

        var users = await userManager.Users.Where(u => ids.Contains(u.Id)).ToListAsync(ct);
        return users.ToDictionary(u => u.Id, Map);
    }

    private static UserSummary Map(ApplicationUser u) =>
        new(u.Id, u.FullName, u.Email ?? "", u.PhoneNumber, u.IsActive);
}
