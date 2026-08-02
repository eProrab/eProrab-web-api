using eProrab.Application.DTOs.Auth;

namespace eProrab.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);

    Task LogoutAsync(string refreshToken, CancellationToken ct = default);

    Task<CurrentUserDto> GetCurrentUserAsync(Guid userId, CancellationToken ct = default);
}
