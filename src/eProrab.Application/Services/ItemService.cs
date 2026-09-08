using eProrab.Application.Common;
using eProrab.Application.DTOs.Items;
using eProrab.Application.Interfaces;
using eProrab.Application.Localization;
using eProrab.Domain.Entities;
using eProrab.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Application.Services;

public class ItemService(IUnitOfWork uow, ILanguageProvider languageProvider) : IItemService
{
    public async Task<PagedResult<ItemDto>> GetPublicPagedAsync(PaginationQuery query, int? categoryId, CancellationToken ct = default)
    {
        var q = uow.Items.Query()
            .Include(i => i.Translations)
            .Include(i => i.Category).ThenInclude(c => c.Translations)
            .Where(i => i.IsActive)
            .AsQueryable();

        if (categoryId.HasValue)
        {
            q = q.Where(i => i.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(i => i.Sku.Contains(term) || i.Translations.Any(t => t.Name.Contains(term)));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "price" => query.SortDescending == true ? q.OrderByDescending(i => i.Price) : q.OrderBy(i => i.Price),
            _ => query.SortDescending == true ? q.OrderByDescending(i => i.CreatedAtUtc) : q.OrderBy(i => i.CreatedAtUtc)
        };

        var page = query.Page ?? 1;
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        var lang = languageProvider.Current;
        return PagedResult<ItemDto>.Create(items.Select(i => ToDto(i, lang)).ToList(), total, page, query.PageSize);
    }

    public async Task<ItemDto> GetPublicByIdAsync(int id, CancellationToken ct = default)
    {
        var item = await uow.Items.Query()
            .Include(i => i.Translations)
            .Include(i => i.Category).ThenInclude(c => c.Translations)
            .FirstOrDefaultAsync(i => i.Id == id && i.IsActive, ct)
            ?? throw new NotFoundException("Item", id);

        return ToDto(item, languageProvider.Current);
    }

    public async Task<PagedResult<ItemAdminDto>> GetPagedForAdminAsync(PaginationQuery query, CancellationToken ct = default)
    {
        var q = uow.Items.Query().Include(i => i.Translations).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            q = q.Where(i => i.Sku.Contains(term) || i.Translations.Any(t => t.Name.Contains(term)));
        }

        q = query.SortBy?.ToLowerInvariant() switch
        {
            "price" => query.SortDescending == true ? q.OrderByDescending(i => i.Price) : q.OrderBy(i => i.Price),
            "sku" => query.SortDescending == true ? q.OrderByDescending(i => i.Sku) : q.OrderBy(i => i.Sku),
            _ => query.SortDescending == true ? q.OrderByDescending(i => i.CreatedAtUtc) : q.OrderBy(i => i.CreatedAtUtc)
        };

        var page = query.Page ?? 1;
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        return PagedResult<ItemAdminDto>.Create(items.Select(ToAdminDto).ToList(), total, page, query.PageSize);
    }

    public async Task<ItemAdminDto> GetByIdForAdminAsync(int id, CancellationToken ct = default)
    {
        var item = await uow.Items.Query().Include(i => i.Translations)
            .FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new NotFoundException("Item", id);

        return ToAdminDto(item);
    }

    public async Task<ItemAdminDto> CreateAsync(CreateItemRequest request, CancellationToken ct = default)
    {
        var categoryExists = await uow.Categories.Query().AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
        {
            throw new NotFoundException("Category", request.CategoryId);
        }

        var skuTaken = await uow.Items.Query().AnyAsync(i => i.Sku == request.Sku, ct);
        if (skuTaken)
        {
            throw new ConflictException(Messages.Get(SystemMessageKey.SkuAlreadyExists, languageProvider.Current));
        }

        var item = new Item
        {
            Sku = request.Sku,
            CategoryId = request.CategoryId,
            Unit = request.Unit,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            ImageUrl = request.ImageUrl,
            IsActive = request.IsActive,
            IsFinishMaterial = request.IsFinishMaterial,
            Dimensions = request.Dimensions,
            MarketUserId = request.MarketUserId,
            MarketName = request.MarketName,
            Translations = request.Translations
                .Select(t => new ItemTranslation { Language = t.Language, Name = t.Name, Description = t.Description })
                .ToList()
        };

        await uow.Items.AddAsync(item, ct);
        await uow.SaveChangesAsync(ct);

        return ToAdminDto(item);
    }

    public async Task<ItemAdminDto> UpdateAsync(int id, UpdateItemRequest request, CancellationToken ct = default)
    {
        var item = await uow.Items.Query().Include(i => i.Translations)
            .FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new NotFoundException("Item", id);

        var categoryExists = await uow.Categories.Query().AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
        {
            throw new NotFoundException("Category", request.CategoryId);
        }

        item.CategoryId = request.CategoryId;
        item.Unit = request.Unit;
        item.Price = request.Price;
        item.StockQuantity = request.StockQuantity;
        item.ImageUrl = request.ImageUrl;
        item.IsActive = request.IsActive;
        item.IsFinishMaterial = request.IsFinishMaterial;
        item.Dimensions = request.Dimensions;
        if (!string.IsNullOrEmpty(request.MarketName))
        {
            item.MarketName = request.MarketName;
        }
        item.UpdatedAtUtc = DateTime.UtcNow;

        foreach (var translation in item.Translations)
        {
            var updated = request.Translations.First(t => t.Language == translation.Language);
            translation.Name = updated.Name;
            translation.Description = updated.Description;
        }

        uow.Items.Update(item);
        await uow.SaveChangesAsync(ct);

        return ToAdminDto(item);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var item = await uow.Items.GetByIdAsync(id, ct) ?? throw new NotFoundException("Item", id);
        item.IsDeleted = true;
        item.UpdatedAtUtc = DateTime.UtcNow;
        uow.Items.Update(item);
        await uow.SaveChangesAsync(ct);
    }

    private static ItemDto ToDto(Item i, Language language)
    {
        var translation = i.Translations.FirstOrDefault(t => t.Language == language)
            ?? i.Translations.FirstOrDefault(t => t.Language == Language.En)
            ?? i.Translations.First();

        return new ItemDto(
            i.Id, i.Sku, i.CategoryId, CategoryService.ResolveName(i.Category.Translations, language),
            i.Unit, i.Price, i.StockQuantity, i.ImageUrl, i.IsActive, i.IsFinishMaterial,
            translation.Name, translation.Description,
            i.Dimensions, i.MarketUserId, i.MarketName, i.SurfaceType);
    }

    private static ItemAdminDto ToAdminDto(Item i) => new(
        i.Id, i.Sku, i.CategoryId, i.Unit, i.Price, i.StockQuantity, i.ImageUrl, i.IsActive, i.IsFinishMaterial,
        i.CreatedAtUtc, i.UpdatedAtUtc,
        i.Translations.Select(t => new ItemTranslationDto(t.Language, t.Name, t.Description)).ToList(),
        i.Dimensions, i.MarketUserId, i.MarketName);
}
