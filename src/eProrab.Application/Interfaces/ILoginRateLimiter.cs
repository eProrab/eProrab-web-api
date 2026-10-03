namespace eProrab.Application.Interfaces;

public record LoginLimiterState(
    int FailedAttempts,
    bool IsLockedOut,
    int? RetryAfterSeconds,
    bool ProgressiveDelayApplied,
    bool RequireCaptcha);

public interface ILoginRateLimiter
{
    /// <summary>
    /// Checks if the IP or account is currently locked out (throws <see cref="Common.TooManyRequestsException"/>).
    /// If >= 3 failed attempts, introduces a progressive delay to thwart automated brute-force attacks.
    /// </summary>
    Task CheckLimitAndDelayAsync(string ip, string account, CancellationToken ct = default);

    /// <summary>
    /// Records a failed login attempt for the given IP and account.
    /// If the consecutive failure threshold (5 attempts within 15 minutes) is met,
    /// locks out both the IP and account for 15 minutes and throws <see cref="Common.TooManyRequestsException"/>.
    /// </summary>
    Task RecordFailedAttemptAsync(string ip, string account, CancellationToken ct = default);

    /// <summary>
    /// Resets the failed attempt counters and lockout for the IP and account upon successful authentication.
    /// </summary>
    Task ResetAttemptsAsync(string ip, string account);

    /// <summary>
    /// Returns current state for monitoring or UI challenge status.
    /// </summary>
    LoginLimiterState GetState(string ip, string account);
}
