using eProrab.Application.Common;
using eProrab.Application.DTOs.Users;

namespace eProrab.Application.Interfaces;

/// <summary>Admin-panel CRUD over accounts. Backed by ASP.NET Core Identity in Infrastructure.</summary>
public interface IUserService
{
    Task<PagedResult<UserDto>> GetPagedAsync(PaginationQuery query, string? role, CancellationToken ct = default);

    Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);

    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);

    Task<UserDto> ChangeRoleAsync(Guid id, ChangeUserRoleRequest request, CancellationToken ct = default);

    Task<UserDto> SetActiveAsync(Guid id, SetUserActiveRequest request, CancellationToken ct = default);

    Task ResetPasswordAsync(Guid id, AdminResetPasswordRequest request, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
