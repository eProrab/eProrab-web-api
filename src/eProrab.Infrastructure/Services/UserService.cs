using eProrab.Application.Common;
using eProrab.Application.DTOs.Users;
using eProrab.Application.Interfaces;
using eProrab.Application.Localization;
using eProrab.Domain.Constants;
using eProrab.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Infrastructure.Services;

public class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ILanguageProvider languageProvider) : IUserService
{
    public async Task<PagedResult<UserDto>> GetPagedAsync(PaginationQuery query, string? role, CancellationToken ct = default)
    {
        var q = userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(u => u.FullName.Contains(term) || (u.Email != null && u.Email.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var idsInRole = (await userManager.GetUsersInRoleAsync(role)).Select(u => u.Id).ToHashSet();
            q = q.Where(u => idsInRole.Contains(u.Id));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.SortDescending == true ? q.OrderByDescending(u => u.FullName) : q.OrderBy(u => u.FullName),
            "email" => query.SortDescending == true ? q.OrderByDescending(u => u.Email) : q.OrderBy(u => u.Email),
            _ => query.SortDescending == true ? q.OrderByDescending(u => u.CreatedAtUtc) : q.OrderBy(u => u.CreatedAtUtc)
        };

        var page = query.Page ?? 1;
        var total = await q.CountAsync(ct);
        var users = await q.Skip((page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        var dtos = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            dtos.Add(await ToDtoAsync(user));
        }

        return PagedResult<UserDto>.Create(dtos, total, page, query.PageSize);
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("User", id);
        return await ToDtoAsync(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        if (await userManager.FindByEmailAsync(request.Email) is not null)
        {
            throw new ConflictException(Messages.Get(SystemMessageKey.EmailAlreadyRegistered, languageProvider.Current));
        }

        if (!await roleManager.RoleExistsAsync(request.Role))
        {
            throw new NotFoundException("Role", request.Role);
        }

        var normalizedPhone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        if (normalizedPhone is not null)
        {
            var phoneExists = await userManager.Users.AnyAsync(u => u.PhoneNumber == normalizedPhone, ct);
            if (phoneExists)
            {
                throw new ConflictException(Messages.Get(SystemMessageKey.PhoneNumberAlreadyRegistered, languageProvider.Current));
            }
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            FullName = request.FullName,
            PhoneNumber = normalizedPhone,
            PreferredLanguage = request.PreferredLanguage,
            IsActive = request.IsActive
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, request.Role);
        return await ToDtoAsync(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("User", id);

        var normalizedPhone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        if (normalizedPhone is not null)
        {
            var phoneExists = await userManager.Users.AnyAsync(u => u.Id != id && u.PhoneNumber == normalizedPhone, ct);
            if (phoneExists)
            {
                throw new ConflictException(Messages.Get(SystemMessageKey.PhoneNumberAlreadyRegistered, languageProvider.Current));
            }
        }

        user.FullName = request.FullName;
        user.PhoneNumber = normalizedPhone;
        user.PreferredLanguage = request.PreferredLanguage;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        return await ToDtoAsync(user);
    }

    public async Task<UserDto> ChangeRoleAsync(Guid id, ChangeUserRoleRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("User", id);

        if (!await roleManager.RoleExistsAsync(request.Role))
        {
            throw new NotFoundException("Role", request.Role);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Contains(Roles.Admin) && request.Role != Roles.Admin)
        {
            await EnsureNotLastAdminAsync(user.Id);
        }

        if (currentRoles.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }
        await userManager.AddToRoleAsync(user, request.Role);

        return await ToDtoAsync(user);
    }

    public async Task<UserDto> SetActiveAsync(Guid id, SetUserActiveRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("User", id);

        if (!request.IsActive && await userManager.IsInRoleAsync(user, Roles.Admin))
        {
            await EnsureNotLastAdminAsync(user.Id);
        }

        user.IsActive = request.IsActive;
        await userManager.UpdateAsync(user);

        return await ToDtoAsync(user);
    }

    public async Task ResetPasswordAsync(Guid id, AdminResetPasswordRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("User", id);

        await userManager.RemovePasswordAsync(user);
        var result = await userManager.AddPasswordAsync(user, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        user.MustChangePassword = true;
        await userManager.UpdateAsync(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(id.ToString()) ?? throw new NotFoundException("User", id);

        if (await userManager.IsInRoleAsync(user, Roles.Admin))
        {
            await EnsureNotLastAdminAsync(user.Id);
        }

        await userManager.DeleteAsync(user);
    }

    private async Task EnsureNotLastAdminAsync(Guid excludingUserId)
    {
        var admins = await userManager.GetUsersInRoleAsync(Roles.Admin);
        var remaining = admins.Count(a => a.Id != excludingUserId && a.IsActive);
        if (remaining < 1)
        {
            throw new ConflictException(Messages.Get(SystemMessageKey.CannotDeleteLastAdmin, languageProvider.Current));
        }
    }

    private async Task<UserDto> ToDtoAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        return new UserDto(user.Id, user.FullName, user.Email!, user.PhoneNumber,
            user.PreferredLanguage, roles.ToList(), user.IsActive, user.CreatedAtUtc, user.MustChangePassword);
    }
}
