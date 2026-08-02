using eProrab.Domain.Enums;

namespace eProrab.Application.DTOs.Jobs;

public record JobPostingDto(
    int Id,
    Guid PostedByUserId,
    string PostedByName,
    int SpecializationId,
    string SpecializationName,
    string Title,
    string Description,
    Language Language,
    string? City,
    decimal? BudgetMin,
    decimal? BudgetMax,
    BudgetType BudgetType,
    DateTime? StartDate,
    int? DurationDays,
    JobStatus Status,
    int ApplicationsCount,
    DateTime CreatedAtUtc);

public record CreateJobPostingRequest(
    int SpecializationId,
    string Title,
    string Description,
    Language Language,
    string? City,
    decimal? BudgetMin,
    decimal? BudgetMax,
    BudgetType BudgetType,
    DateTime? StartDate,
    int? DurationDays);

public record UpdateJobPostingRequest(
    int SpecializationId,
    string Title,
    string Description,
    Language Language,
    string? City,
    decimal? BudgetMin,
    decimal? BudgetMax,
    BudgetType BudgetType,
    DateTime? StartDate,
    int? DurationDays,
    JobStatus Status);

public record JobApplicationDto(
    int Id,
    int JobPostingId,
    string JobPostingTitle,
    int WorkerProfileId,
    string WorkerFullName,
    string WorkerSpecializationName,
    string? CoverMessage,
    decimal? ProposedRate,
    ApplicationStatus Status,
    DateTime AppliedAtUtc,
    DateTime? RespondedAtUtc,
    decimal? AgreedRate,
    DateTime? AgreedStartDate);

/// <summary>Submitted by a worker, from their cabinet, against an open job posting.</summary>
public record CreateJobApplicationRequest(string? CoverMessage, decimal? ProposedRate);

/// <summary>Submitted by the employer (job owner or Admin) to hire the applicant.</summary>
public record AcceptJobApplicationRequest(decimal? AgreedRate, DateTime? AgreedStartDate);
