using eProrab.Application.Common;
using eProrab.Application.DTOs.Workers;
using eProrab.Domain.Enums;

namespace eProrab.Application.Interfaces;

public interface IWorkerService
{
    /// <summary>Public/employer-facing browse of available workers, optionally filtered by specialization/city/workerType.</summary>
    Task<PagedResult<WorkerProfileDto>> BrowseAsync(PaginationQuery query, int? specializationId, string? city, WorkerType? workerType = null, CancellationToken ct = default);

    Task<WorkerProfileDto> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Worker cabinet: get the caller's own profile, or null if not created yet.</summary>
    Task<WorkerProfileDto?> GetOwnProfileAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Worker cabinet: create the caller's profile (one per user).</summary>
    Task<WorkerProfileDto> CreateOwnProfileAsync(Guid userId, UpsertWorkerProfileRequest request, CancellationToken ct = default);

    /// <summary>Worker cabinet: update the caller's own profile.</summary>
    Task<WorkerProfileDto> UpdateOwnProfileAsync(Guid userId, UpsertWorkerProfileRequest request, CancellationToken ct = default);

    Task<PagedResult<WorkerProfileAdminDto>> GetPagedForAdminAsync(PaginationQuery query, WorkerType? workerType = null, CancellationToken ct = default);

    Task<WorkerProfileAdminDto> SetVerifiedAsync(int id, bool isVerified, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}

