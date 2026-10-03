using eProrab.Domain.Enums;

namespace eProrab.Application.DTOs.Users;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    Language PreferredLanguage,
    IReadOnlyList<string> Roles,
    bool IsActive,
    DateTime CreatedAtUtc,
    bool MustChangePassword = false);

/// <summary>Admin-only: create a user directly with a chosen role, skipping self-registration.</summary>
public record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    string? PhoneNumber,
    Language PreferredLanguage,
    string Role,
    bool IsActive);

public record UpdateUserRequest(
    string FullName,
    string? PhoneNumber,
    Language PreferredLanguage);

public record ChangeUserRoleRequest(string Role);

public record SetUserActiveRequest(bool IsActive);

public record AdminResetPasswordRequest(string NewPassword);
