using eProrab.Domain.Enums;
using FluentValidation;

namespace eProrab.Application.Validators;

/// <summary>
/// Shared FluentValidation rule for admin-curated content (Item, Category,
/// Specialization): every request must supply exactly one translation per
/// supported language, each with a non-empty name — guaranteeing the catalog
/// is always fully trilingual, never partially translated.
/// </summary>
public static class TranslationRuleExtensions
{
    public static IRuleBuilderOptions<T, IReadOnlyList<TTranslation>> MustCoverAllLanguages<T, TTranslation>(
        this IRuleBuilder<T, IReadOnlyList<TTranslation>> ruleBuilder,
        Func<TTranslation, Language> languageSelector,
        Func<TTranslation, string?> nameSelector)
    {
        return ruleBuilder
            .Must(translations => translations is not null && translations.Count == 3)
            .WithMessage("Exactly 3 translations (az, en, ru) are required.")
            .Must(translations => translations
                .Select(languageSelector)
                .Distinct()
                .Count() == 3)
            .WithMessage("Translations must cover az, en and ru exactly once each, with no duplicates.")
            .Must(translations => translations.All(t => !string.IsNullOrWhiteSpace(nameSelector(t))))
            .WithMessage("Every translation must have a non-empty name.");
    }
}
