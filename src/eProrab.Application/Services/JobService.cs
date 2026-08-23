using eProrab.Application.Common;
using eProrab.Application.DTOs.Jobs;
using eProrab.Application.Interfaces;
using eProrab.Application.Localization;
using eProrab.Domain.Entities;
using eProrab.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Application.Services;

public class JobService(IUnitOfWork uow, IUserDirectoryService userDirectory, ILanguageProvider languageProvider) : IJobService
{
    // ---------------------------------------------------------------- browse

    public async Task<PagedResult<JobPostingDto>> BrowseOpenAsync(PaginationQuery query, int? specializationId, string? city, CancellationToken ct = default)
    {
        var q = BaseJobQuery().Where(j => j.Status == JobStatus.Open);

        if (specializationId.HasValue)
        {
            q = q.Where(j => j.SpecializationId == specializationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            q = q.Where(j => j.City != null && j.City.Contains(city));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(j => j.Title.Contains(term) || j.Description.Contains(term));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "budget" => query.SortDescending == true ? q.OrderByDescending(j => j.BudgetMax) : q.OrderBy(j => j.BudgetMax),
            _ => query.SortDescending == true ? q.OrderByDescending(j => j.CreatedAtUtc) : q.OrderBy(j => j.CreatedAtUtc)
        };

        var page = query.Page ?? 1;
        var total = await q.CountAsync(ct);
        var jobs = await q.Skip((page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return PagedResult<JobPostingDto>.Create(await MapJobsAsync(jobs, ct), total, page, query.PageSize);
    }

    public async Task<JobPostingDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var job = await BaseJobQuery().FirstOrDefaultAsync(j => j.Id == id, ct)
            ?? throw new NotFoundException("JobPosting", id);

        return (await MapJobsAsync([job], ct))[0];
    }

    // ---------------------------------------------------------------- employer side

    public async Task<JobPostingDto> CreateAsync(Guid postedByUserId, CreateJobPostingRequest request, CancellationToken ct = default)
    {
        var specializationExists = await uow.Specializations.Query().AnyAsync(s => s.Id == request.SpecializationId, ct);
        if (!specializationExists)
        {
            throw new NotFoundException("Specialization", request.SpecializationId);
        }

        var job = new JobPosting
        {
            PostedByUserId = postedByUserId,
            SpecializationId = request.SpecializationId,
            Title = request.Title,
            Description = request.Description,
            Language = request.Language,
            City = request.City,
            BudgetMin = request.BudgetMin,
            BudgetMax = request.BudgetMax,
            BudgetType = request.BudgetType,
            StartDate = request.StartDate,
            DurationDays = request.DurationDays,
            Status = JobStatus.Open
        };

        await uow.JobPostings.AddAsync(job, ct);
        await uow.SaveChangesAsync(ct);

        return await GetByIdAsync(job.Id, ct);
    }

    public async Task<JobPostingDto> UpdateAsync(int id, Guid callerUserId, bool callerIsAdmin, UpdateJobPostingRequest request, CancellationToken ct = default)
    {
        var job = await uow.JobPostings.Query().FirstOrDefaultAsync(j => j.Id == id, ct)
            ?? throw new NotFoundException("JobPosting", id);

        EnsureOwnerOrAdmin(job.PostedByUserId, callerUserId, callerIsAdmin);

        var specializationExists = await uow.Specializations.Query().AnyAsync(s => s.Id == request.SpecializationId, ct);
        if (!specializationExists)
        {
            throw new NotFoundException("Specialization", request.SpecializationId);
        }

        job.SpecializationId = request.SpecializationId;
        job.Title = request.Title;
        job.Description = request.Description;
        job.Language = request.Language;
        job.City = request.City;
        job.BudgetMin = request.BudgetMin;
        job.BudgetMax = request.BudgetMax;
        job.BudgetType = request.BudgetType;
        job.StartDate = request.StartDate;
        job.DurationDays = request.DurationDays;
        job.Status = request.Status;
        job.UpdatedAtUtc = DateTime.UtcNow;

        uow.JobPostings.Update(job);
        await uow.SaveChangesAsync(ct);

        return await GetByIdAsync(job.Id, ct);
    }

    public async Task DeleteAsync(int id, Guid callerUserId, bool callerIsAdmin, CancellationToken ct = default)
    {
        var job = await uow.JobPostings.GetByIdAsync(id, ct) ?? throw new NotFoundException("JobPosting", id);
        EnsureOwnerOrAdmin(job.PostedByUserId, callerUserId, callerIsAdmin);

        job.IsDeleted = true;
        job.Status = JobStatus.Cancelled;
        job.UpdatedAtUtc = DateTime.UtcNow;
        uow.JobPostings.Update(job);
        await uow.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<JobPostingDto>> GetOwnPostingsAsync(Guid postedByUserId, PaginationQuery query, CancellationToken ct = default)
    {
        var q = BaseJobQuery().Where(j => j.PostedByUserId == postedByUserId).OrderByDescending(j => j.CreatedAtUtc);

        var page = query.Page ?? 1;
        var total = await q.CountAsync(ct);
        var jobs = await q.Skip((page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return PagedResult<JobPostingDto>.Create(await MapJobsAsync(jobs, ct), total, page, query.PageSize);
    }

    public async Task<IReadOnlyList<JobApplicationDto>> GetApplicantsAsync(int jobPostingId, Guid callerUserId, bool callerIsAdmin, CancellationToken ct = default)
    {
        var job = await uow.JobPostings.Query().FirstOrDefaultAsync(j => j.Id == jobPostingId, ct)
            ?? throw new NotFoundException("JobPosting", jobPostingId);

        EnsureOwnerOrAdmin(job.PostedByUserId, callerUserId, callerIsAdmin);

        var applications = await BaseApplicationQuery()
            .Where(a => a.JobPostingId == jobPostingId)
            .OrderByDescending(a => a.AppliedAtUtc)
            .ToListAsync(ct);

        return await MapApplicationsAsync(applications, ct);
    }

    public async Task<JobApplicationDto> AcceptApplicationAsync(int jobPostingId, int applicationId, Guid callerUserId, bool callerIsAdmin, AcceptJobApplicationRequest request, CancellationToken ct = default)
    {
        var job = await uow.JobPostings.Query().FirstOrDefaultAsync(j => j.Id == jobPostingId, ct)
            ?? throw new NotFoundException("JobPosting", jobPostingId);

        EnsureOwnerOrAdmin(job.PostedByUserId, callerUserId, callerIsAdmin);

        if (job.Status != JobStatus.Open)
        {
            throw new ConflictException(Messages.Get(SystemMessageKey.JobPostingNotOpen, languageProvider.Current));
        }

        var application = await uow.JobApplications.Query()
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPostingId == jobPostingId, ct)
            ?? throw new NotFoundException("JobApplication", applicationId);

        application.Status = ApplicationStatus.Accepted;
        application.AgreedRate = request.AgreedRate ?? application.ProposedRate;
        application.AgreedStartDate = request.AgreedStartDate;
        application.RespondedAtUtc = DateTime.UtcNow;
        uow.JobApplications.Update(application);

        // Auto-decline every other still-pending application for this job.
        var others = await uow.JobApplications.Query()
            .Where(a => a.JobPostingId == jobPostingId && a.Id != applicationId &&
                        (a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Reviewed))
            .ToListAsync(ct);
        foreach (var other in others)
        {
            other.Status = ApplicationStatus.Rejected;
            other.RespondedAtUtc = DateTime.UtcNow;
            uow.JobApplications.Update(other);
        }

        job.Status = JobStatus.Filled;
        job.UpdatedAtUtc = DateTime.UtcNow;
        uow.JobPostings.Update(job);

        await uow.SaveChangesAsync(ct);

        var mapped = await BaseApplicationQuery().FirstAsync(a => a.Id == applicationId, ct);
        return (await MapApplicationsAsync([mapped], ct))[0];
    }

    public async Task<JobApplicationDto> RejectApplicationAsync(int jobPostingId, int applicationId, Guid callerUserId, bool callerIsAdmin, CancellationToken ct = default)
    {
        var job = await uow.JobPostings.Query().FirstOrDefaultAsync(j => j.Id == jobPostingId, ct)
            ?? throw new NotFoundException("JobPosting", jobPostingId);

        EnsureOwnerOrAdmin(job.PostedByUserId, callerUserId, callerIsAdmin);

        var application = await uow.JobApplications.Query()
            .FirstOrDefaultAsync(a => a.Id == applicationId && a.JobPostingId == jobPostingId, ct)
            ?? throw new NotFoundException("JobApplication", applicationId);

        application.Status = ApplicationStatus.Rejected;
        application.RespondedAtUtc = DateTime.UtcNow;
        uow.JobApplications.Update(application);
        await uow.SaveChangesAsync(ct);

        var mapped = await BaseApplicationQuery().FirstAsync(a => a.Id == applicationId, ct);
        return (await MapApplicationsAsync([mapped], ct))[0];
    }

    // ---------------------------------------------------------------- worker cabinet side

    public async Task<JobApplicationDto> ApplyAsync(int jobPostingId, Guid workerUserId, CreateJobApplicationRequest request, CancellationToken ct = default)
    {
        var workerProfile = await uow.WorkerProfiles.Query().FirstOrDefaultAsync(w => w.UserId == workerUserId, ct)
            ?? throw new NotFoundException("WorkerProfile", workerUserId);

        var job = await uow.JobPostings.Query().FirstOrDefaultAsync(j => j.Id == jobPostingId, ct)
            ?? throw new NotFoundException("JobPosting", jobPostingId);

        if (job.Status != JobStatus.Open)
        {
            throw new ConflictException(Messages.Get(SystemMessageKey.JobPostingNotOpen, languageProvider.Current));
        }

        if (job.PostedByUserId == workerUserId)
        {
            throw new ConflictException(Messages.Get(SystemMessageKey.CannotApplyToOwnJob, languageProvider.Current));
        }

        var alreadyApplied = await uow.JobApplications.Query().AnyAsync(a =>
            a.JobPostingId == jobPostingId && a.WorkerProfileId == workerProfile.Id &&
            a.Status != ApplicationStatus.Withdrawn && a.Status != ApplicationStatus.Rejected, ct);
        if (alreadyApplied)
        {
            throw new ConflictException(Messages.Get(SystemMessageKey.AlreadyAppliedToJob, languageProvider.Current));
        }

        var application = new JobApplication
        {
            JobPostingId = jobPostingId,
            WorkerProfileId = workerProfile.Id,
            CoverMessage = request.CoverMessage,
            ProposedRate = request.ProposedRate,
            Status = ApplicationStatus.Pending,
            AppliedAtUtc = DateTime.UtcNow
        };

        await uow.JobApplications.AddAsync(application, ct);
        await uow.SaveChangesAsync(ct);

        var mapped = await BaseApplicationQuery().FirstAsync(a => a.Id == application.Id, ct);
        return (await MapApplicationsAsync([mapped], ct))[0];
    }

    public async Task<IReadOnlyList<JobApplicationDto>> GetOwnApplicationsAsync(Guid workerUserId, CancellationToken ct = default)
    {
        var workerProfile = await uow.WorkerProfiles.Query().FirstOrDefaultAsync(w => w.UserId == workerUserId, ct)
            ?? throw new NotFoundException("WorkerProfile", workerUserId);

        var applications = await BaseApplicationQuery()
            .Where(a => a.WorkerProfileId == workerProfile.Id)
            .OrderByDescending(a => a.AppliedAtUtc)
            .ToListAsync(ct);

        return await MapApplicationsAsync(applications, ct);
    }

    public async Task WithdrawApplicationAsync(int applicationId, Guid workerUserId, CancellationToken ct = default)
    {
        var application = await uow.JobApplications.Query().Include(a => a.WorkerProfile)
            .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new NotFoundException("JobApplication", applicationId);

        if (application.WorkerProfile.UserId != workerUserId)
        {
            throw new ForbiddenException();
        }

        application.Status = ApplicationStatus.Withdrawn;
        application.RespondedAtUtc = DateTime.UtcNow;
        uow.JobApplications.Update(application);
        await uow.SaveChangesAsync(ct);
    }

    // ---------------------------------------------------------------- admin

    public async Task<PagedResult<JobPostingDto>> GetPagedForAdminAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var q = BaseJobQuery().OrderByDescending(j => j.CreatedAtUtc);

        var page = query.Page ?? 1;
        var total = await q.CountAsync(ct);
        var jobs = await q.Skip((page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return PagedResult<JobPostingDto>.Create(await MapJobsAsync(jobs, ct), total, page, query.PageSize);
    }

    // ---------------------------------------------------------------- helpers

    private static void EnsureOwnerOrAdmin(Guid ownerUserId, Guid callerUserId, bool callerIsAdmin)
    {
        if (!callerIsAdmin && ownerUserId != callerUserId)
        {
            throw new ForbiddenException();
        }
    }

    private IQueryable<JobPosting> BaseJobQuery() => uow.JobPostings.Query()
        .Include(j => j.Specialization).ThenInclude(s => s.Translations)
        .Include(j => j.Applications);

    private IQueryable<JobApplication> BaseApplicationQuery() => uow.JobApplications.Query()
        .Include(a => a.JobPosting)
        .Include(a => a.WorkerProfile).ThenInclude(w => w.Specialization).ThenInclude(s => s.Translations);

    private async Task<List<JobPostingDto>> MapJobsAsync(IReadOnlyList<JobPosting> jobs, CancellationToken ct)
    {
        var names = await userDirectory.GetSummariesAsync(jobs.Select(j => j.PostedByUserId).Distinct(), ct);
        var lang = languageProvider.Current;

        return jobs.Select(j => new JobPostingDto(
            j.Id, j.PostedByUserId, names.GetValueOrDefault(j.PostedByUserId)?.FullName ?? "Unknown",
            j.SpecializationId, SpecializationService.ResolveName(j.Specialization.Translations, lang),
            j.Title, j.Description, j.Language, j.City, j.BudgetMin, j.BudgetMax, j.BudgetType,
            j.StartDate, j.DurationDays, j.Status, j.Applications.Count, j.CreatedAtUtc)).ToList();
    }

    private async Task<List<JobApplicationDto>> MapApplicationsAsync(IReadOnlyList<JobApplication> applications, CancellationToken ct)
    {
        var names = await userDirectory.GetSummariesAsync(applications.Select(a => a.WorkerProfile.UserId).Distinct(), ct);
        var lang = languageProvider.Current;

        return applications.Select(a => new JobApplicationDto(
            a.Id, a.JobPostingId, a.JobPosting.Title, a.WorkerProfileId,
            names.GetValueOrDefault(a.WorkerProfile.UserId)?.FullName ?? "Unknown",
            SpecializationService.ResolveName(a.WorkerProfile.Specialization.Translations, lang),
            a.CoverMessage, a.ProposedRate, a.Status, a.AppliedAtUtc, a.RespondedAtUtc,
            a.AgreedRate, a.AgreedStartDate)).ToList();
    }
}
