namespace eProrab.Application.DTOs.Workers;

/// <summary>Public/employer-facing view of a worker, shown when browsing candidates.</summary>
public record WorkerProfileDto(
    int Id,
    Guid UserId,
    string FullName,
    int SpecializationId,
    string SpecializationName,
    int ExperienceYears,
    string? Bio,
    string? City,
    decimal? DailyRate,
    bool IsAvailableForHire,
    bool IsVerified);

/// <summary>Admin view — adds contact info hidden from public browsing.</summary>
public record WorkerProfileAdminDto(
    int Id,
    Guid UserId,
    string FullName,
    string Email,
    string? PhoneNumber,
    int SpecializationId,
    string SpecializationName,
    int ExperienceYears,
    string? Bio,
    string? City,
    decimal? DailyRate,
    bool IsAvailableForHire,
    bool IsVerified,
    DateTime CreatedAtUtc);

/// <summary>Worker-cabinet self-service create/update of the caller's own profile.</summary>
public record UpsertWorkerProfileRequest(
    int SpecializationId,
    int ExperienceYears,
    string? Bio,
    string? City,
    decimal? DailyRate,
    bool IsAvailableForHire);
