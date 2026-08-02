using eProrab.Domain.Enums;

namespace eProrab.Application.DTOs.Categories;

public record CategoryTranslationDto(Language Language, string Name);

public record CategoryDto(int Id, string Slug, int DisplayOrder, bool IsActive, string Name);

public record CategoryAdminDto(
    int Id,
    string Slug,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<CategoryTranslationDto> Translations);

public record CreateCategoryRequest(
    string Slug,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<CategoryTranslationDto> Translations);

public record UpdateCategoryRequest(
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<CategoryTranslationDto> Translations);
