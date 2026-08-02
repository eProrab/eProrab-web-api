namespace eProrab.Application.Interfaces;

/// <summary>Read-only view of the authenticated caller, derived from JWT claims.</summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }

    IReadOnlyList<string> Roles { get; }

    bool IsInRole(string role);
}
