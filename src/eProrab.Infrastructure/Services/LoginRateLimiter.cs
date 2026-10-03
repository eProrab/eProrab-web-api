using System.Collections.Concurrent;
using eProrab.Application.Common;
using eProrab.Application.Interfaces;
using eProrab.Application.Localization;
using eProrab.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eProrab.Infrastructure.Services;

public class LoginRateLimiter(
    IServiceScopeFactory scopeFactory,
    ILogger<LoginRateLimiter> logger) : ILoginRateLimiter
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan AttemptWindow = TimeSpan.FromMinutes(15);
    private const int ProgressiveDelayThreshold = 3;

    private class AttemptRecord
    {
        public int FailedCount { get; set; }
        public DateTime? LockoutEndUtc { get; set; }
        public DateTime LastAttemptUtc { get; set; } = DateTime.UtcNow;
    }

    private readonly ConcurrentDictionary<string, AttemptRecord> _records = new(StringComparer.OrdinalIgnoreCase);

    public async Task CheckLimitAndDelayAsync(string ip, string account, CancellationToken ct = default)
    {
        CleanupOldRecords();

        var ipKey = GetIpKey(ip);
        var accKey = GetAccountKey(account);

        var ipRecord = GetOrNull(ipKey);
        var accRecord = GetOrNull(accKey);

        var now = DateTime.UtcNow;

        // 1. Check if either IP or Account is locked out
        DateTime? activeLockoutEnd = null;
        if (ipRecord?.LockoutEndUtc is not null && ipRecord.LockoutEndUtc > now)
        {
            activeLockoutEnd = ipRecord.LockoutEndUtc;
        }
        if (accRecord?.LockoutEndUtc is not null && accRecord.LockoutEndUtc > now)
        {
            if (activeLockoutEnd is null || accRecord.LockoutEndUtc > activeLockoutEnd)
            {
                activeLockoutEnd = accRecord.LockoutEndUtc;
            }
        }

        if (activeLockoutEnd.HasValue)
        {
            var remaining = activeLockoutEnd.Value - now;
            var retryAfterSec = Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds));
            var remainingMin = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));

            logger.LogWarning("Login blocked by rate limiter for IP: {Ip}, Account: {Account}. Locked until {LockoutEnd} (remaining: {Seconds}s)",
                ip, account, activeLockoutEnd.Value, retryAfterSec);

            var message = FormatLockoutMessage(remainingMin, retryAfterSec);
            throw new TooManyRequestsException(message, retryAfterSec);
        }

        // 2. Check for Progressive Delay if failed attempts >= 3
        var maxFailures = Math.Max(ipRecord?.FailedCount ?? 0, accRecord?.FailedCount ?? 0);
        if (maxFailures >= ProgressiveDelayThreshold)
        {
            // 3 failed -> 1500ms delay, 4 failed -> 3000ms delay
            var delayMs = maxFailures == 3 ? 1500 : 3000;
            logger.LogInformation("Applying progressive delay of {DelayMs}ms for IP: {Ip}, Account: {Account} (failures: {Failures})",
                delayMs, ip, account, maxFailures);

            await Task.Delay(delayMs, ct);
        }
    }

    public Task RecordFailedAttemptAsync(string ip, string account, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var ipKey = GetIpKey(ip);
        var accKey = GetAccountKey(account);

        var updatedIp = IncrementAttempt(ipKey, now);
        var updatedAcc = IncrementAttempt(accKey, now);

        var maxFailures = Math.Max(updatedIp.FailedCount, updatedAcc.FailedCount);

        // If >= 5 consecutive failed attempts, lock out for 15 minutes!
        if (maxFailures >= MaxFailedAttempts)
        {
            var lockoutUntil = now.Add(LockoutDuration);
            updatedIp.LockoutEndUtc = lockoutUntil;
            updatedAcc.LockoutEndUtc = lockoutUntil;

            var retryAfterSec = (int)LockoutDuration.TotalSeconds;
            var remainingMin = (int)LockoutDuration.TotalMinutes;

            logger.LogWarning("Account/IP reached {Max} failed attempts. Locked out for 15 minutes. IP: {Ip}, Account: {Account}",
                maxFailures, ip, account);

            var message = FormatLockoutMessage(remainingMin, retryAfterSec);
            throw new TooManyRequestsException(message, retryAfterSec);
        }

        return Task.CompletedTask;
    }

    public Task ResetAttemptsAsync(string ip, string account)
    {
        var ipKey = GetIpKey(ip);
        var accKey = GetAccountKey(account);

        _records.TryRemove(ipKey, out _);
        _records.TryRemove(accKey, out _);

        return Task.CompletedTask;
    }

    public LoginLimiterState GetState(string ip, string account)
    {
        var now = DateTime.UtcNow;
        var ipRecord = GetOrNull(GetIpKey(ip));
        var accRecord = GetOrNull(GetAccountKey(account));

        var maxFailures = Math.Max(ipRecord?.FailedCount ?? 0, accRecord?.FailedCount ?? 0);
        var isLocked = (ipRecord?.LockoutEndUtc is not null && ipRecord.LockoutEndUtc > now) ||
                       (accRecord?.LockoutEndUtc is not null && accRecord.LockoutEndUtc > now);

        int? retryAfter = null;
        if (isLocked)
        {
            var end = (ipRecord?.LockoutEndUtc > accRecord?.LockoutEndUtc ? ipRecord?.LockoutEndUtc : accRecord?.LockoutEndUtc) ?? now;
            retryAfter = Math.Max(1, (int)Math.Ceiling((end - now).TotalSeconds));
        }

        return new LoginLimiterState(
            FailedAttempts: maxFailures,
            IsLockedOut: isLocked,
            RetryAfterSeconds: retryAfter,
            ProgressiveDelayApplied: maxFailures >= ProgressiveDelayThreshold,
            RequireCaptcha: maxFailures >= ProgressiveDelayThreshold
        );
    }

    private AttemptRecord IncrementAttempt(string key, DateTime now)
    {
        return _records.AddOrUpdate(
            key,
            _ => new AttemptRecord { FailedCount = 1, LastAttemptUtc = now },
            (_, existing) =>
            {
                // If previous attempt was outside the 15-minute window and not locked out, reset counter
                if (now - existing.LastAttemptUtc > AttemptWindow && (existing.LockoutEndUtc is null || existing.LockoutEndUtc <= now))
                {
                    existing.FailedCount = 1;
                }
                else
                {
                    existing.FailedCount++;
                }

                existing.LastAttemptUtc = now;
                return existing;
            });
    }

    private AttemptRecord? GetOrNull(string key)
    {
        if (_records.TryGetValue(key, out var record))
        {
            // If expired outside attempt window and not locked out, ignore
            if (DateTime.UtcNow - record.LastAttemptUtc > AttemptWindow &&
                (record.LockoutEndUtc is null || record.LockoutEndUtc <= DateTime.UtcNow))
            {
                _records.TryRemove(key, out _);
                return null;
            }
            return record;
        }
        return null;
    }

    private void CleanupOldRecords()
    {
        var now = DateTime.UtcNow;
        if (_records.Count < 500) return; // Only cleanup when dictionary grows

        foreach (var (key, record) in _records)
        {
            if (now - record.LastAttemptUtc > AttemptWindow &&
                (record.LockoutEndUtc is null || record.LockoutEndUtc <= now))
            {
                _records.TryRemove(key, out _);
            }
        }
    }

    private string FormatLockoutMessage(int minutes, int seconds)
    {
        using var scope = scopeFactory.CreateScope();
        var lang = scope.ServiceProvider.GetRequiredService<ILanguageProvider>().Current;
        var minText = minutes <= 1 ? "1" : minutes.ToString();

        return lang switch
        {
            Language.Az => $"15 dəqiqə ərzində 5 dəfə ardıcıl yanlış şifrə daxil edildiyi üçün giriş 15 dəqiqəlik bloklanıb. Zəhmət olmasa {minText} dəqiqə sonra yenidən cəhd edin.",
            Language.Ru => $"Вход заблокирован на 15 минут из-за 5 подряд неверных попыток ввода пароля. Пожалуйста, повторите попытку через {minText} мин.",
            _ => $"Access has been locked for 15 minutes due to 5 consecutive failed attempts. Please try again in {minText} minute(s)."
        };
    }

    private static string GetIpKey(string ip) => $"ip:{ip.Trim()}";
    private static string GetAccountKey(string account) => $"acc:{account.Trim().ToLowerInvariant()}";
}
