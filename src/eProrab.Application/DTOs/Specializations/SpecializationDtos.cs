using eProrab.Domain.Enums;

namespace eProrab.Application.DTOs.Specializations;

public record SpecializationTranslationDto(Language Language, string Name);

public record SpecializationDto(int Id, string Slug, int DisplayOrder, bool IsActive, string Name);

public record SpecializationAdminDto(
    int Id,
    string Slug,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<SpecializationTranslationDto> Translations);

public record CreateSpecializationRequest(
    string Slug,
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<SpecializationTranslationDto> Translations);

public record UpdateSpecializationRequest(
    int DisplayOrder,
    bool IsActive,
    IReadOnlyList<SpecializationTranslationDto> Translations);
