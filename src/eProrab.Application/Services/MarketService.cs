using eProrab.Application.Common;
using eProrab.Application.DTOs.Markets;
using eProrab.Application.Interfaces;
using eProrab.Domain.Entities;
using eProrab.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace eProrab.Application.Services;

public class MarketService(IUnitOfWork uow, ILanguageProvider languageProvider) : IMarketService
{
    public async Task<MarketProfileDto?> GetOwnProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await uow.MarketProfiles.Query()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile is null) return null;

        var itemsCount = await uow.Items.Query().CountAsync(i => i.MarketUserId == userId, ct);
        return ToDto(profile, itemsCount);
    }

    public async Task<MarketProfileDto> CreateOwnProfileAsync(Guid userId, UpsertMarketProfileRequest request, CancellationToken ct = default)
    {
        var existing = await uow.MarketProfiles.Query().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (existing is not null)
        {
            throw new ConflictException("Market profili artıq mövcuddur.");
        }

        var profile = new MarketProfile
        {
            UserId = userId,
            StoreName = request.StoreName.Trim(),
            Voen = request.Voen?.Trim(),
            Description = request.Description?.Trim(),
            ContactPhone = request.ContactPhone?.Trim(),
            ContactEmail = request.ContactEmail?.Trim(),
            Address = request.Address?.Trim(),
            City = request.City?.Trim(),
            LogoUrl = request.LogoUrl?.Trim(),
            BannerUrl = request.BannerUrl?.Trim(),
            WorkingHours = request.WorkingHours?.Trim(),
            IsActive = true,
            IsVerified = false
        };

        await uow.MarketProfiles.AddAsync(profile, ct);
        await uow.SaveChangesAsync(ct);

        return ToDto(profile, 0);
    }

    public async Task<MarketProfileDto> UpdateOwnProfileAsync(Guid userId, UpsertMarketProfileRequest request, CancellationToken ct = default)
    {
        var profile = await uow.MarketProfiles.Query().FirstOrDefaultAsync(p => p.UserId == userId, ct)
            ?? throw new NotFoundException("MarketProfile", userId);

        profile.StoreName = request.StoreName.Trim();
        profile.Voen = request.Voen?.Trim();
        profile.Description = request.Description?.Trim();
        profile.ContactPhone = request.ContactPhone?.Trim();
        profile.ContactEmail = request.ContactEmail?.Trim();
        profile.Address = request.Address?.Trim();
        profile.City = request.City?.Trim();
        profile.LogoUrl = request.LogoUrl?.Trim();
        profile.BannerUrl = request.BannerUrl?.Trim();
        profile.WorkingHours = request.WorkingHours?.Trim();
        profile.UpdatedAtUtc = DateTime.UtcNow;

        uow.MarketProfiles.Update(profile);

        // Also update MarketName on all items owned by this market
        var items = await uow.Items.Query().Where(i => i.MarketUserId == userId).ToListAsync(ct);
        foreach (var itm in items)
        {
            itm.MarketName = profile.StoreName;
        }

        await uow.SaveChangesAsync(ct);

        var itemsCount = await uow.Items.Query().CountAsync(i => i.MarketUserId == userId, ct);
        return ToDto(profile, itemsCount);
    }

    public async Task<PagedResult<MarketItemDto>> GetMyItemsAsync(Guid userId, PaginationQuery query, int? categoryId, CancellationToken ct = default)
    {
        var q = uow.Items.Query()
            .Include(i => i.Translations)
            .Include(i => i.Category).ThenInclude(c => c.Translations)
            .Where(i => i.MarketUserId == userId)
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
            "sku" => query.SortDescending == true ? q.OrderByDescending(i => i.Sku) : q.OrderBy(i => i.Sku),
            _ => query.SortDescending == true ? q.OrderByDescending(i => i.CreatedAtUtc) : q.OrderBy(i => i.CreatedAtUtc)
        };

        var page = query.Page ?? 1;
        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);

        var lang = languageProvider.Current;
        var dtos = items.Select(i => ToMarketItemDto(i, lang)).ToList();

        return PagedResult<MarketItemDto>.Create(dtos, total, page, query.PageSize);
    }

    public async Task<MarketItemDto> CreateItemAsync(Guid userId, CreateMarketItemRequest request, CancellationToken ct = default)
    {
        var category = await uow.Categories.Query().FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct)
            ?? throw new NotFoundException("Category", request.CategoryId);

        var marketProfile = await uow.MarketProfiles.Query().FirstOrDefaultAsync(m => m.UserId == userId, ct);
        var marketName = marketProfile?.StoreName ?? "Mağaza";

        var sku = !string.IsNullOrWhiteSpace(request.Sku)
            ? request.Sku.Trim().ToUpperInvariant()
            : $"MKT-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        var item = new Item
        {
            Sku = sku,
            CategoryId = request.CategoryId,
            Unit = request.Unit,
            Price = request.Price,
            StockQuantity = request.StockQuantity ?? 100,
            ImageUrl = request.ImageUrl?.Trim(),
            Dimensions = request.Dimensions?.Trim(),
            IsActive = true,
            IsFinishMaterial = request.IsFinishMaterial,
            SurfaceType = request.SurfaceType,
            MarketUserId = userId,
            MarketName = marketName,
            Translations =
            [
                new ItemTranslation { Language = Language.Az, Name = request.Name.Trim(), Description = request.Description?.Trim() },
                new ItemTranslation { Language = Language.En, Name = request.Name.Trim(), Description = request.Description?.Trim() },
                new ItemTranslation { Language = Language.Ru, Name = request.Name.Trim(), Description = request.Description?.Trim() },
            ]
        };

        await uow.Items.AddAsync(item, ct);
        await uow.SaveChangesAsync(ct);

        return ToMarketItemDto(item, languageProvider.Current);
    }

    public async Task<MarketItemDto> UpdateItemAsync(Guid userId, int itemId, UpdateMarketItemRequest request, CancellationToken ct = default)
    {
        var item = await uow.Items.Query()
            .Include(i => i.Translations)
            .Include(i => i.Category).ThenInclude(c => c.Translations)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.MarketUserId == userId, ct)
            ?? throw new NotFoundException("Item", itemId);

        var categoryExists = await uow.Categories.Query().AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
        {
            throw new NotFoundException("Category", request.CategoryId);
        }

        item.CategoryId = request.CategoryId;
        item.Unit = request.Unit;
        item.Price = request.Price;
        item.StockQuantity = request.StockQuantity;
        item.ImageUrl = request.ImageUrl?.Trim();
        item.Dimensions = request.Dimensions?.Trim();
        item.IsActive = request.IsActive;
        item.IsFinishMaterial = request.IsFinishMaterial;
        item.SurfaceType = request.SurfaceType;
        item.UpdatedAtUtc = DateTime.UtcNow;

        foreach (var translation in item.Translations)
        {
            translation.Name = request.Name.Trim();
            translation.Description = request.Description?.Trim();
        }

        uow.Items.Update(item);
        await uow.SaveChangesAsync(ct);

        return ToMarketItemDto(item, languageProvider.Current);
    }

    public async Task DeleteItemAsync(Guid userId, int itemId, CancellationToken ct = default)
    {
        var item = await uow.Items.Query().FirstOrDefaultAsync(i => i.Id == itemId && i.MarketUserId == userId, ct)
            ?? throw new NotFoundException("Item", itemId);

        item.IsDeleted = true;
        item.UpdatedAtUtc = DateTime.UtcNow;
        uow.Items.Update(item);
        await uow.SaveChangesAsync(ct);
    }

    private static MarketProfileDto ToDto(MarketProfile p, int itemsCount) => new(
        p.Id,
        p.UserId,
        p.StoreName,
        p.Voen,
        p.Description,
        p.ContactPhone,
        p.ContactEmail,
        p.Address,
        p.City,
        p.LogoUrl,
        p.BannerUrl,
        p.IsVerified,
        p.IsActive,
        p.WorkingHours,
        itemsCount,
        p.CreatedAtUtc);

    private static MarketItemDto ToMarketItemDto(Item i, Language language)
    {
        var translation = i.Translations.FirstOrDefault(t => t.Language == language)
            ?? i.Translations.FirstOrDefault(t => t.Language == Language.En)
            ?? i.Translations.FirstOrDefault()
            ?? new ItemTranslation { Name = "Material" };

        var catName = i.Category != null
            ? (i.Category.Translations.FirstOrDefault(t => t.Language == language)?.Name ?? "Kateqoriya")
            : "Kateqoriya";

        return new MarketItemDto(
            i.Id,
            i.Sku,
            i.CategoryId,
            catName,
            i.Unit,
            i.Price,
            i.StockQuantity,
            i.ImageUrl,
            i.IsActive,
            i.IsFinishMaterial,
            translation.Name,
            translation.Description,
            i.Dimensions,
            i.SurfaceType,
            i.CreatedAtUtc);
    }
}
