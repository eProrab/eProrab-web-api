using eProrab.Application.Common;
using eProrab.Application.DTOs.Workers;
using eProrab.Application.Interfaces;
using eProrab.Application.Localization;
using eProrab.Domain.Entities;
using eProrab.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Application.Services;

public class WorkerService(IUnitOfWork uow, IUserDirectoryService userDirectory, ILanguageProvider languageProvider) : IWorkerService
{
    public async Task<PagedResult<WorkerProfileDto>> BrowseAsync(PaginationQuery query, int? specializationId, string? city, WorkerType? workerType = null, bool? isArchitectTeamMember = null, CancellationToken ct = default)
    {
        var q = uow.WorkerProfiles.Query()
            .Include(w => w.Specialization).ThenInclude(s => s.Translations)
            .Where(w => w.IsAvailableForHire)
            .AsQueryable();

        if (workerType.HasValue)
        {
            q = q.Where(w => w.WorkerType == workerType.Value);
        }

        if (isArchitectTeamMember.HasValue)
        {
            q = q.Where(w => w.IsArchitectTeamMember == isArchitectTeamMember.Value);
        }

        if (specializationId.HasValue)
        {
            q = q.Where(w => w.SpecializationId == specializationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            q = q.Where(w => w.City != null && w.City.Contains(city));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "experience" => query.SortDescending == true ? q.OrderByDescending(w => w.ExperienceYears) : q.OrderBy(w => w.ExperienceYears),
            "rate" => query.SortDescending == true ? q.OrderByDescending(w => w.DailyRate) : q.OrderBy(w => w.DailyRate),
            _ => query.SortDescending == true ? q.OrderByDescending(w => w.CreatedAtUtc) : q.OrderBy(w => w.CreatedAtUtc)
        };

        var page = query.Page ?? 1;
        var total = await q.CountAsync(ct);
        var profiles = await q.Skip((page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        var names = await userDirectory.GetSummariesAsync(profiles.Select(p => p.UserId), ct);
        var lang = languageProvider.Current;

        var dtos = profiles.Select(p => ToDto(p, names.GetValueOrDefault(p.UserId)?.FullName ?? "Unknown", lang)).ToList();
        return PagedResult<WorkerProfileDto>.Create(dtos, total, page, query.PageSize);
    }

    public async Task<WorkerProfileDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var profile = await uow.WorkerProfiles.Query()
            .Include(w => w.Specialization).ThenInclude(s => s.Translations)
            .FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new NotFoundException("WorkerProfile", id);

        var summary = await userDirectory.GetSummaryAsync(profile.UserId, ct);
        return ToDto(profile, summary?.FullName ?? "Unknown", languageProvider.Current);
    }

    public async Task<WorkerProfileDto?> GetOwnProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.WorkerProfiles.Query()
            .Include(w => w.Specialization).ThenInclude(s => s.Translations)
            .FirstOrDefaultAsync(w => w.UserId == userId, ct);

        if (profile is null) return null;

        var summary = await userDirectory.GetSummaryAsync(userId, ct);
        return ToDto(profile, summary?.FullName ?? "Unknown", languageProvider.Current);
    }

    public async Task<WorkerProfileDto> CreateOwnProfileAsync(Guid userId, UpsertWorkerProfileRequest request, CancellationToken ct = default)
    {
        var exists = await uow.WorkerProfiles.Query().AnyAsync(w => w.UserId == userId, ct);
        if (exists)
        {
            throw new ConflictException(Messages.Get(SystemMessageKey.WorkerProfileAlreadyExists, languageProvider.Current));
        }

        var specializationExists = await uow.Specializations.Query().AnyAsync(s => s.Id == request.SpecializationId, ct);
        if (!specializationExists)
        {
            throw new NotFoundException("Specialization", request.SpecializationId);
        }

        var profile = new WorkerProfile
        {
            UserId = userId,
            WorkerType = request.WorkerType,
            CompanyName = request.CompanyName,
            Voen = request.Voen,
            TeamSize = request.TeamSize,
            SpecializationId = request.SpecializationId,
            ExperienceYears = request.ExperienceYears,
            Bio = request.Bio,
            City = request.City,
            DailyRate = request.DailyRate,
            IsAvailableForHire = request.IsAvailableForHire,
            IsArchitectTeamMember = request.IsArchitectTeamMember,
            ArchitectName = request.ArchitectName,
            ArchitectStudio = request.ArchitectStudio
        };

        await uow.WorkerProfiles.AddAsync(profile, ct);
        await uow.SaveChangesAsync(ct);

        return (await GetOwnProfileAsync(userId, ct))!;
    }

    public async Task<WorkerProfileDto> UpdateOwnProfileAsync(Guid userId, UpsertWorkerProfileRequest request, CancellationToken ct = default)
    {
        var profile = await uow.WorkerProfiles.Query().FirstOrDefaultAsync(w => w.UserId == userId, ct)
            ?? throw new NotFoundException("WorkerProfile", userId);

        var specializationExists = await uow.Specializations.Query().AnyAsync(s => s.Id == request.SpecializationId, ct);
        if (!specializationExists)
        {
            throw new NotFoundException("Specialization", request.SpecializationId);
        }

        profile.WorkerType = request.WorkerType;
        profile.CompanyName = request.CompanyName;
        profile.Voen = request.Voen;
        profile.TeamSize = request.TeamSize;
        profile.SpecializationId = request.SpecializationId;
        profile.ExperienceYears = request.ExperienceYears;
        profile.Bio = request.Bio;
        profile.City = request.City;
        profile.DailyRate = request.DailyRate;
        profile.IsAvailableForHire = request.IsAvailableForHire;
        profile.IsArchitectTeamMember = request.IsArchitectTeamMember;
        profile.ArchitectName = request.ArchitectName;
        profile.ArchitectStudio = request.ArchitectStudio;
        profile.UpdatedAtUtc = DateTime.UtcNow;

        uow.WorkerProfiles.Update(profile);
        await uow.SaveChangesAsync(ct);

        return (await GetOwnProfileAsync(userId, ct))!;
    }

    public async Task<PagedResult<WorkerProfileAdminDto>> GetPagedForAdminAsync(PaginationQuery query, WorkerType? workerType = null, bool? isArchitectTeamMember = null, CancellationToken ct = default)
    {
        var q = uow.WorkerProfiles.Query().Include(w => w.Specialization).ThenInclude(s => s.Translations).AsQueryable();

        if (workerType.HasValue)
        {
            q = q.Where(w => w.WorkerType == workerType.Value);
        }

        if (isArchitectTeamMember.HasValue)
        {
            q = q.Where(w => w.IsArchitectTeamMember == isArchitectTeamMember.Value);
        }

        var page = query.Page ?? 1;
        var total = await q.CountAsync(ct);
        var profiles = await q.OrderByDescending(w => w.CreatedAtUtc)
            .Skip((page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        var names = await userDirectory.GetSummariesAsync(profiles.Select(p => p.UserId), ct);
        var lang = languageProvider.Current;

        var dtos = profiles.Select(p =>
        {
            var summary = names.GetValueOrDefault(p.UserId);
            return new WorkerProfileAdminDto(
                p.Id, p.UserId, summary?.FullName ?? "Unknown", summary?.Email ?? "", summary?.PhoneNumber,
                p.WorkerType, p.CompanyName, p.Voen, p.TeamSize,
                p.SpecializationId, SpecializationService.ResolveName(p.Specialization.Translations, lang),
                p.ExperienceYears, p.Bio, p.City, p.DailyRate, p.IsAvailableForHire, p.IsVerified, p.CreatedAtUtc,
                p.IsArchitectTeamMember, p.ArchitectName, p.ArchitectStudio);
        }).ToList();

        return PagedResult<WorkerProfileAdminDto>.Create(dtos, total, page, query.PageSize);
    }

    public async Task<WorkerProfileAdminDto> SetVerifiedAsync(int id, bool isVerified, CancellationToken ct = default)
    {
        var profile = await uow.WorkerProfiles.Query().Include(w => w.Specialization).ThenInclude(s => s.Translations)
            .FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new NotFoundException("WorkerProfile", id);

        profile.IsVerified = isVerified;
        profile.UpdatedAtUtc = DateTime.UtcNow;
        uow.WorkerProfiles.Update(profile);
        await uow.SaveChangesAsync(ct);

        var summary = await userDirectory.GetSummaryAsync(profile.UserId, ct);
        var lang = languageProvider.Current;
        return new WorkerProfileAdminDto(
            profile.Id, profile.UserId, summary?.FullName ?? "Unknown", summary?.Email ?? "", summary?.PhoneNumber,
            profile.WorkerType, profile.CompanyName, profile.Voen, profile.TeamSize,
            profile.SpecializationId, SpecializationService.ResolveName(profile.Specialization.Translations, lang),
            profile.ExperienceYears, profile.Bio, profile.City, profile.DailyRate,
            profile.IsAvailableForHire, profile.IsVerified, profile.CreatedAtUtc,
            profile.IsArchitectTeamMember, profile.ArchitectName, profile.ArchitectStudio);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var profile = await uow.WorkerProfiles.GetByIdAsync(id, ct) ?? throw new NotFoundException("WorkerProfile", id);
        profile.IsDeleted = true;
        profile.UpdatedAtUtc = DateTime.UtcNow;
        uow.WorkerProfiles.Update(profile);
        await uow.SaveChangesAsync(ct);
    }

    private static WorkerProfileDto ToDto(WorkerProfile p, string fullName, Domain.Enums.Language language) => new(
        p.Id, p.UserId, fullName, p.WorkerType, p.CompanyName, p.Voen, p.TeamSize, p.SpecializationId,
        SpecializationService.ResolveName(p.Specialization.Translations, language),
        p.ExperienceYears, p.Bio, p.City, p.DailyRate, p.IsAvailableForHire, p.IsVerified,
        p.IsArchitectTeamMember, p.ArchitectName, p.ArchitectStudio);
}

