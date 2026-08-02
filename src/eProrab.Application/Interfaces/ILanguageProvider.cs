using eProrab.Domain.Enums;

namespace eProrab.Application.Interfaces;

/// <summary>
/// Resolves the language of the current request (from the Accept-Language
/// header, an explicit ?lang= query parameter, or the user's saved preference).
/// Implemented in the API layer, where HttpContext is available.
/// </summary>
public interface ILanguageProvider
{
    Language Current { get; }
}
