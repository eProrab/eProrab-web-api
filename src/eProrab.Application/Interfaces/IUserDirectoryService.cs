namespace eProrab.Application.Interfaces;

public record UserSummary(Guid Id, string FullName, string Email, string? PhoneNumber, bool IsActive);

/// <summary>
/// Read-only lookup of Identity user display info, so Application services that
/// only need a name/email (e.g. showing "posted by" on a job, or a worker's
/// contact info to an admin) don't need to reference Identity types directly.
/// Implemented in Infrastructure via UserManager&lt;ApplicationUser&gt;.
/// </summary>
public interface IUserDirectoryService
{
    Task<UserSummary?> GetSummaryAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, UserSummary>> GetSummariesAsync(IEnumerable<Guid> userIds, CancellationToken ct = default);
}
