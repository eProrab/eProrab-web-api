using eProrab.Application.Common;
using eProrab.Application.DTOs.Auth;
using eProrab.Application.Interfaces;
using eProrab.Application.Localization;
using eProrab.Domain.Constants;
using eProrab.Domain.Entities;
using eProrab.Infrastructure.Identity;
using eProrab.Infrastructure.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace eProrab.Infrastructure.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IUnitOfWork uow,
    ITokenService tokenService,
    ILanguageProvider languageProvider,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            throw new ConflictException(Messages.Get(SystemMessageKey.EmailAlreadyRegistered, languageProvider.Current));
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            PreferredLanguage = request.PreferredLanguage,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        var role = string.Equals(request.Role, Roles.Worker, StringComparison.OrdinalIgnoreCase)
            ? Roles.Worker
            : string.Equals(request.Role, Roles.Architect, StringComparison.OrdinalIgnoreCase)
                ? Roles.Architect
                : Roles.Client;

        await userManager.AddToRoleAsync(user, role);

        return await IssueTokensAsync(user, ct);
    }


    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedAccessException(Messages.Get(SystemMessageKey.InvalidCredentials, languageProvider.Current));
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(Messages.Get(SystemMessageKey.AccountDeactivated, languageProvider.Current));
        }

        return await IssueTokensAsync(user, ct);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await uow.RefreshTokens.Query().FirstOrDefaultAsync(t => t.Token == refreshToken, ct);
        if (stored is null || !stored.IsActive)
        {
            throw new UnauthorizedAccessException(Messages.Get(SystemMessageKey.InvalidOrExpiredRefreshToken, languageProvider.Current));
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException(Messages.Get(SystemMessageKey.InvalidOrExpiredRefreshToken, languageProvider.Current));
        }

        var newRefreshTokenValue = tokenService.GenerateRefreshToken();
        stored.RevokedAtUtc = DateTime.UtcNow;
        stored.ReplacedByToken = newRefreshTokenValue;
        uow.RefreshTokens.Update(stored);

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        };
        await uow.RefreshTokens.AddAsync(newRefreshToken, ct);
        await uow.SaveChangesAsync(ct);

        var roles = await userManager.GetRolesAsync(user);
        var access = tokenService.GenerateAccessToken(user.Id, user.Email!, user.FullName, roles);

        return new AuthResponse(access.Token, access.ExpiresAtUtc, newRefreshTokenValue, await GetCurrentUserAsync(user.Id, ct));
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        var stored = await uow.RefreshTokens.Query().FirstOrDefaultAsync(t => t.Token == refreshToken, ct);
        if (stored is null || !stored.IsActive)
        {
            return; // Already invalid/expired — logout is idempotent.
        }

        stored.RevokedAtUtc = DateTime.UtcNow;
        uow.RefreshTokens.Update(stored);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        var roles = await userManager.GetRolesAsync(user);
        var hasWorkerProfile = await uow.WorkerProfiles.Query().AnyAsync(w => w.UserId == userId, ct);

        return new CurrentUserDto(
            user.Id, user.FullName, user.Email!, user.PhoneNumber,
            user.PreferredLanguage, roles.ToList(), user.IsActive, hasWorkerProfile);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task<CurrentUserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber?.Trim();

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        return await GetCurrentUserAsync(userId, ct);
    }

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, CancellationToken ct)
    {
        var roles = await userManager.GetRolesAsync(user);
        var access = tokenService.GenerateAccessToken(user.Id, user.Email!, user.FullName, roles);
        var refreshTokenValue = tokenService.GenerateRefreshToken();

        await uow.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays)
        }, ct);
        await uow.SaveChangesAsync(ct);

        return new AuthResponse(access.Token, access.ExpiresAtUtc, refreshTokenValue, await GetCurrentUserAsync(user.Id, ct));
    }
}
