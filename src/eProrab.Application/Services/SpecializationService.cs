using eProrab.Application.Common;
using eProrab.Application.DTOs.Specializations;
using eProrab.Application.Interfaces;
using eProrab.Application.Localization;
using eProrab.Domain.Entities;
using eProrab.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Application.Services;

public class SpecializationService(IUnitOfWork uow, ILanguageProvider languageProvider) : ISpecializationService
{
    public async Task<IReadOnlyList<SpecializationDto>> GetActiveAsync(CancellationToken ct = default)
    {
        var specializations = await uow.Specializations.Query()
            .Include(s => s.Translations)
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(ct);

        return specializations.Select(s => ToDto(s, languageProvider.Current)).ToList();
    }

    public async Task<PagedResult<SpecializationAdminDto>> GetPagedForAdminAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var q = uow.Specializations.Query().Include(s => s.Translations).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(s => s.Slug.Contains(term) || s.Translations.Any(t => t.Name.Contains(term)));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "slug" => query.SortDescending == true ? q.OrderByDescending(s => s.Slug) : q.OrderBy(s => s.Slug),
            _ => query.SortDescending == true ? q.OrderByDescending(s => s.DisplayOrder) : q.OrderBy(s => s.DisplayOrder)
        };

        var page = query.Page ?? 1;
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return PagedResult<SpecializationAdminDto>.Create(items.Select(ToAdminDto).ToList(), total, page, query.PageSize);
    }

    public async Task<SpecializationAdminDto> GetByIdForAdminAsync(int id, CancellationToken ct = default)
    {
        var specialization = await uow.Specializations.Query().Include(s => s.Translations)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Specialization", id);

        return ToAdminDto(specialization);
    }

    public async Task<SpecializationAdminDto> CreateAsync(CreateSpecializationRequest request, CancellationToken ct = default)
    {
        var slugTaken = await uow.Specializations.Query().AnyAsync(s => s.Slug == request.Slug, ct);
        if (slugTaken)
        {
            throw new ConflictException(Messages.Get(SystemMessageKey.SlugAlreadyExists, languageProvider.Current));
        }

        var specialization = new Specialization
        {
            Slug = request.Slug,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            Translations = request.Translations
                .Select(t => new SpecializationTranslation { Language = t.Language, Name = t.Name })
                .ToList()
        };

        await uow.Specializations.AddAsync(specialization, ct);
        await uow.SaveChangesAsync(ct);

        return ToAdminDto(specialization);
    }

    public async Task<SpecializationAdminDto> UpdateAsync(int id, UpdateSpecializationRequest request, CancellationToken ct = default)
    {
        var specialization = await uow.Specializations.Query().Include(s => s.Translations)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("Specialization", id);

        specialization.DisplayOrder = request.DisplayOrder;
        specialization.IsActive = request.IsActive;
        specialization.UpdatedAtUtc = DateTime.UtcNow;

        foreach (var translation in specialization.Translations)
        {
            var updated = request.Translations.First(t => t.Language == translation.Language);
            translation.Name = updated.Name;
        }

        uow.Specializations.Update(specialization);
        await uow.SaveChangesAsync(ct);

        return ToAdminDto(specialization);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var specialization = await uow.Specializations.GetByIdAsync(id, ct) ?? throw new NotFoundException("Specialization", id);
        specialization.IsDeleted = true;
        specialization.UpdatedAtUtc = DateTime.UtcNow;
        uow.Specializations.Update(specialization);
        await uow.SaveChangesAsync(ct);
    }

    private static SpecializationDto ToDto(Specialization s, Language language) => new(
        s.Id, s.Slug, s.DisplayOrder, s.IsActive, ResolveName(s.Translations, language));

    private static SpecializationAdminDto ToAdminDto(Specialization s) => new(
        s.Id, s.Slug, s.DisplayOrder, s.IsActive,
        s.Translations.Select(t => new SpecializationTranslationDto(t.Language, t.Name)).ToList());

    internal static string ResolveName(IEnumerable<SpecializationTranslation> translations, Language language)
    {
        var list = translations.ToList();
        return list.FirstOrDefault(t => t.Language == language)?.Name
            ?? list.FirstOrDefault(t => t.Language == Language.En)?.Name
            ?? list.First().Name;
    }
}
