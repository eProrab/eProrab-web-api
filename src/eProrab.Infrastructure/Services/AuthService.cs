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
    IOAuthTokenValidator oauthValidator,
    ILoginRateLimiter loginRateLimiter,
    Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    private string GetClientIp()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null) return "127.0.0.1";

        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var ip = forwarded.ToString().Split(',')[0].Trim();
            if (!string.IsNullOrEmpty(ip)) return ip;
        }

        if (httpContext.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            var ip = realIp.ToString().Trim();
            if (!string.IsNullOrEmpty(ip)) return ip;
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
        {
            throw new ConflictException(Messages.Get(SystemMessageKey.EmailAlreadyRegistered, languageProvider.Current));
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
            FullName = request.FullName,
            PhoneNumber = normalizedPhone,
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
                : string.Equals(request.Role, Roles.Market, StringComparison.OrdinalIgnoreCase)
                    ? Roles.Market
                    : Roles.Client;

        await userManager.AddToRoleAsync(user, role);

        return await IssueTokensAsync(user, ct);
    }


    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim();
        var clientIp = GetClientIp();

        // 1. Check if IP or Account is currently locked out & apply progressive delay (>= 3 attempts)
        await loginRateLimiter.CheckLimitAndDelayAsync(clientIp, email, ct);

        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        {
            // Record failed attempt for IP & Account.
            // Throws TooManyRequestsException (429) if threshold (5 attempts in 15 min) is met.
            await loginRateLimiter.RecordFailedAttemptAsync(clientIp, email, ct);

            throw new UnauthorizedAccessException(Messages.Get(SystemMessageKey.InvalidCredentials, languageProvider.Current));
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(Messages.Get(SystemMessageKey.AccountDeactivated, languageProvider.Current));
        }

        // Reset failed attempt counters on successful login
        await loginRateLimiter.ResetAttemptsAsync(clientIp, email);

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
        var hasMarketProfile = await uow.MarketProfiles.Query().AnyAsync(m => m.UserId == userId, ct);

        return new CurrentUserDto(
            user.Id, user.FullName, user.Email!, user.PhoneNumber,
            user.PreferredLanguage, roles.ToList(), user.IsActive, hasWorkerProfile, hasMarketProfile,
            user.MustChangePassword);
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

        if (user.MustChangePassword)
        {
            user.MustChangePassword = false;
            await userManager.UpdateAsync(user);
        }
    }

    public async Task<CurrentUserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        var normalizedPhone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        if (normalizedPhone is not null)
        {
            var phoneExists = await userManager.Users.AnyAsync(u => u.Id != userId && u.PhoneNumber == normalizedPhone, ct);
            if (phoneExists)
            {
                throw new ConflictException(Messages.Get(SystemMessageKey.PhoneNumberAlreadyRegistered, languageProvider.Current));
            }
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = normalizedPhone;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        return await GetCurrentUserAsync(userId, ct);
    }

    public async Task<AuthResponse> SetUserRoleAsync(Guid userId, string role, CancellationToken ct = default)
    {
        var validRole = string.Equals(role, Roles.Worker, StringComparison.OrdinalIgnoreCase)
            ? Roles.Worker
            : string.Equals(role, Roles.Architect, StringComparison.OrdinalIgnoreCase)
                ? Roles.Architect
                : string.Equals(role, Roles.Market, StringComparison.OrdinalIgnoreCase)
                    ? Roles.Market
                    : string.Equals(role, Roles.Client, StringComparison.OrdinalIgnoreCase)
                        ? Roles.Client
                        : throw new ConflictException("Only Client, Worker, Architect, or Market roles are allowed.");

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        var currentRoles = await userManager.GetRolesAsync(user);

        if (currentRoles.Contains(Roles.Admin) && validRole != Roles.Admin)
        {
            var admins = await userManager.GetUsersInRoleAsync(Roles.Admin);
            if (admins.Count(a => a.Id != user.Id && a.IsActive) == 0)
            {
                throw new ConflictException("Cannot change role: the system must have at least one active Admin.");
            }
        }

        if (currentRoles.Count > 0)
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await userManager.AddToRoleAsync(user, validRole);

        return await IssueTokensAsync(user, ct);
    }

    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, CancellationToken ct, bool isNewAccount = false)
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

        return new AuthResponse(access.Token, access.ExpiresAtUtc, refreshTokenValue, await GetCurrentUserAsync(user.Id, ct), isNewAccount);
    }

    public async Task<AuthResponse> GoogleLoginAsync(OAuthLoginRequest request, CancellationToken ct = default)
    {
        var (googleId, email, fullName) = await oauthValidator.ValidateGoogleTokenAsync(request.IdToken, ct);

        // Check if user exists with this Google ID
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId, ct);

        if (user is not null)
        {
            // User exists with this Google ID - existing user, just log them in
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException(Messages.Get(SystemMessageKey.AccountDeactivated, languageProvider.Current));
            }

            return await IssueTokensAsync(user, ct, isNewAccount: false);
        }

        // Check if email already exists
        var existingByEmail = await userManager.FindByEmailAsync(email);
        if (existingByEmail is not null)
        {
            // Email exists but not linked to Google - auto-link and log in (not a new account)
            if (!existingByEmail.IsActive)
            {
                throw new UnauthorizedAccessException(Messages.Get(SystemMessageKey.AccountDeactivated, languageProvider.Current));
            }

            existingByEmail.GoogleId = googleId;
            existingByEmail.GoogleLinkedAtUtc = DateTime.UtcNow;
            await userManager.UpdateAsync(existingByEmail);

            return await IssueTokensAsync(existingByEmail, ct, isNewAccount: false);
        }

        // Auto-create brand-new user with Google credentials
        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            PhoneNumber = request.PhoneNumber,
            PreferredLanguage = request.PreferredLanguage,
            GoogleId = googleId,
            GoogleLinkedAtUtc = DateTime.UtcNow,
            IsActive = true,
            EmailConfirmed = true // OAuth emails are pre-verified
        };

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        // Assign Client role by default (user will select their actual role)
        await userManager.AddToRoleAsync(user, Roles.Client);

        return await IssueTokensAsync(user, ct, isNewAccount: true);
    }

    public async Task<AuthResponse> FacebookLoginAsync(OAuthLoginRequest request, CancellationToken ct = default)
    {
        var (facebookId, email, fullName) = await oauthValidator.ValidateFacebookTokenAsync(request.IdToken, ct);

        // Check if user exists with this Facebook ID
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.FacebookId == facebookId, ct);

        if (user is not null)
        {
            // User exists with this Facebook ID - existing user, just log them in
            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException(Messages.Get(SystemMessageKey.AccountDeactivated, languageProvider.Current));
            }

            return await IssueTokensAsync(user, ct, isNewAccount: false);
        }

        // Check if email already exists
        var existingByEmail = await userManager.FindByEmailAsync(email);
        if (existingByEmail is not null)
        {
            // Email exists but not linked to Facebook - auto-link and log in (not a new account)
            if (!existingByEmail.IsActive)
            {
                throw new UnauthorizedAccessException(Messages.Get(SystemMessageKey.AccountDeactivated, languageProvider.Current));
            }

            existingByEmail.FacebookId = facebookId;
            existingByEmail.FacebookLinkedAtUtc = DateTime.UtcNow;
            await userManager.UpdateAsync(existingByEmail);

            return await IssueTokensAsync(existingByEmail, ct, isNewAccount: false);
        }

        // Auto-create new user with Facebook credentials
        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            PhoneNumber = request.PhoneNumber,
            PreferredLanguage = request.PreferredLanguage,
            FacebookId = facebookId,
            FacebookLinkedAtUtc = DateTime.UtcNow,
            IsActive = true,
            EmailConfirmed = true // OAuth emails are pre-verified
        };

        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        // Assign Client role by default (user will select their actual role)
        await userManager.AddToRoleAsync(user, Roles.Client);

        return await IssueTokensAsync(user, ct, isNewAccount: true);
    }

    public async Task LinkGoogleAsync(Guid userId, LinkOAuthProviderRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(Messages.Get(SystemMessageKey.AccountDeactivated, languageProvider.Current));
        }

        var (googleId, _, _) = await oauthValidator.ValidateGoogleTokenAsync(request.IdToken, ct);

        // Check if this Google ID is already linked to another account
        var existing = await userManager.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId && u.Id != userId, ct);
        if (existing is not null)
        {
            throw new ConflictException("This Google account is already linked to another user account.");
        }

        // Check if this user already has a Google link
        if (user.GoogleId is not null)
        {
            throw new ConflictException("Your account is already linked to a Google account.");
        }

        user.GoogleId = googleId;
        user.GoogleLinkedAtUtc = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task LinkFacebookAsync(Guid userId, LinkOAuthProviderRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User", userId);

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(Messages.Get(SystemMessageKey.AccountDeactivated, languageProvider.Current));
        }

        var (facebookId, _, _) = await oauthValidator.ValidateFacebookTokenAsync(request.IdToken, ct);

        // Check if this Facebook ID is already linked to another account
        var existing = await userManager.Users.FirstOrDefaultAsync(u => u.FacebookId == facebookId && u.Id != userId, ct);
        if (existing is not null)
        {
            throw new ConflictException("This Facebook account is already linked to another user account.");
        }

        // Check if this user already has a Facebook link
        if (user.FacebookId is not null)
        {
            throw new ConflictException("Your account is already linked to a Facebook account.");
        }

        user.FacebookId = facebookId;
        user.FacebookLinkedAtUtc = DateTime.UtcNow;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
