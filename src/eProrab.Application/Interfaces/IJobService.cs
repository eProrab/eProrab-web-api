using eProrab.Application.Common;
using eProrab.Application.DTOs.Jobs;

namespace eProrab.Application.Interfaces;

public interface IJobService
{
    // --- Public / employer browsing ---
    Task<PagedResult<JobPostingDto>> BrowseOpenAsync(PaginationQuery query, int? specializationId, string? city, CancellationToken ct = default);

    Task<JobPostingDto> GetByIdAsync(int id, CancellationToken ct = default);

    // --- Employer (Client/Manager/Admin) side ---
    Task<JobPostingDto> CreateAsync(Guid postedByUserId, CreateJobPostingRequest request, CancellationToken ct = default);

    Task<JobPostingDto> UpdateAsync(int id, Guid callerUserId, bool callerIsAdmin, UpdateJobPostingRequest request, CancellationToken ct = default);

    Task DeleteAsync(int id, Guid callerUserId, bool callerIsAdmin, CancellationToken ct = default);

    Task<PagedResult<JobPostingDto>> GetOwnPostingsAsync(Guid postedByUserId, PaginationQuery query, CancellationToken ct = default);

    Task<IReadOnlyList<JobApplicationDto>> GetApplicantsAsync(int jobPostingId, Guid callerUserId, bool callerIsAdmin, CancellationToken ct = default);

    Task<JobApplicationDto> AcceptApplicationAsync(int jobPostingId, int applicationId, Guid callerUserId, bool callerIsAdmin, AcceptJobApplicationRequest request, CancellationToken ct = default);

    Task<JobApplicationDto> RejectApplicationAsync(int jobPostingId, int applicationId, Guid callerUserId, bool callerIsAdmin, CancellationToken ct = default);

    // --- Worker cabinet side ---
    Task<JobApplicationDto> ApplyAsync(int jobPostingId, Guid workerUserId, CreateJobApplicationRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<JobApplicationDto>> GetOwnApplicationsAsync(Guid workerUserId, CancellationToken ct = default);

    Task WithdrawApplicationAsync(int applicationId, Guid workerUserId, CancellationToken ct = default);

    // --- Admin moderation ---
    Task<PagedResult<JobPostingDto>> GetPagedForAdminAsync(PaginationQuery query, CancellationToken ct = default);
}
