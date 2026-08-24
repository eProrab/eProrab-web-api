using eProrab.Domain.Enums;

namespace eProrab.Application.DTOs.Workers;

/// <summary>Public/employer-facing view of a worker, shown when browsing candidates.</summary>
public record WorkerProfileDto(
    int Id,
    Guid UserId,
    string FullName,
    WorkerType WorkerType,
    string? CompanyName,
    string? Voen,
    int? TeamSize,
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
    WorkerType WorkerType,
    string? CompanyName,
    string? Voen,
    int? TeamSize,
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
    WorkerType WorkerType,
    string? CompanyName,
    string? Voen,
    int? TeamSize,
    int SpecializationId,
    int ExperienceYears,
    string? Bio,
    string? City,
    decimal? DailyRate,
    bool IsAvailableForHire);

