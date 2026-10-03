using eProrab.Application.Common;
using eProrab.Application.DTOs.Categories;
using eProrab.Application.Interfaces;
using eProrab.Application.Localization;
using eProrab.Domain.Entities;
using eProrab.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Application.Services;

public class CategoryService(IUnitOfWork uow, ILanguageProvider languageProvider) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryDto>> GetActiveAsync(CancellationToken ct = default)
    {
        var categories = await uow.Categories.Query()
            .Include(c => c.Translations)
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);

        return categories.Select(c => ToDto(c, languageProvider.Current)).ToList();
    }

    public async Task<PagedResult<CategoryAdminDto>> GetPagedForAdminAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var q = uow.Categories.Query().Include(c => c.Translations).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(c => c.Slug.Contains(term) || c.Translations.Any(t => t.Name.Contains(term)));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "slug" => query.SortDescending == true ? q.OrderByDescending(c => c.Slug) : q.OrderBy(c => c.Slug),
            _ => query.SortDescending == true ? q.OrderByDescending(c => c.DisplayOrder) : q.OrderBy(c => c.DisplayOrder)
        };

        var page = query.Page ?? 1;
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return PagedResult<CategoryAdminDto>.Create(items.Select(ToAdminDto).ToList(), total, page, query.PageSize);
    }

    public async Task<CategoryAdminDto> GetByIdForAdminAsync(int id, CancellationToken ct = default)
    {
        var category = await uow.Categories.Query().Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Category", id);

        return ToAdminDto(category);
    }

    public async Task<CategoryAdminDto> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        var slugTaken = await uow.Categories.Query().AnyAsync(c => c.Slug == request.Slug, ct);
        if (slugTaken)
        {
            throw new ConflictException(Messages.Get(SystemMessageKey.SlugAlreadyExists, languageProvider.Current));
        }

        var category = new Category
        {
            Slug = request.Slug,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            Translations = request.Translations
                .Select(t => new CategoryTranslation { Language = t.Language, Name = t.Name })
                .ToList()
        };

        await uow.Categories.AddAsync(category, ct);
        await uow.SaveChangesAsync(ct);

        return ToAdminDto(category);
    }

    public async Task<CategoryAdminDto> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await uow.Categories.Query().Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Category", id);

        if (!string.IsNullOrWhiteSpace(request.Slug) && request.Slug != category.Slug)
        {
            var slugTaken = await uow.Categories.Query().AnyAsync(c => c.Slug == request.Slug && c.Id != id, ct);
            if (slugTaken)
                throw new ConflictException(Messages.Get(SystemMessageKey.SlugAlreadyExists, languageProvider.Current));
            category.Slug = request.Slug;
        }

        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAtUtc = DateTime.UtcNow;

        foreach (var reqTranslation in request.Translations)
        {
            var existing = category.Translations.FirstOrDefault(t => t.Language == reqTranslation.Language);
            if (existing is not null)
            {
                existing.Name = reqTranslation.Name;
            }
            else
            {
                category.Translations.Add(new CategoryTranslation
                {
                    CategoryId = category.Id,
                    Language = reqTranslation.Language,
                    Name = reqTranslation.Name
                });
            }
        }

        uow.Categories.Update(category);
        await uow.SaveChangesAsync(ct);

        return ToAdminDto(category);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await uow.Categories.GetByIdAsync(id, ct) ?? throw new NotFoundException("Category", id);
        category.IsDeleted = true;
        category.UpdatedAtUtc = DateTime.UtcNow;
        uow.Categories.Update(category);
        await uow.SaveChangesAsync(ct);
    }

    private static CategoryDto ToDto(Category c, Language language) => new(
        c.Id, c.Slug, c.DisplayOrder, c.IsActive, ResolveName(c.Translations, language));

    private static CategoryAdminDto ToAdminDto(Category c) => new(
        c.Id, c.Slug, c.DisplayOrder, c.IsActive,
        c.Translations.Select(t => new CategoryTranslationDto(t.Language, t.Name)).ToList());

    internal static string ResolveName(IEnumerable<CategoryTranslation> translations, Language language)
    {
        var list = translations.ToList();
        return list.FirstOrDefault(t => t.Language == language)?.Name
            ?? list.FirstOrDefault(t => t.Language == Language.En)?.Name
            ?? list.First().Name;
    }
}
