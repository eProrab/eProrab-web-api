using eProrab.Application.Interfaces;
using eProrab.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace eProrab.Infrastructure.Services;

/// <summary>
/// Resolves the request language with priority: explicit ?lang= query param
/// → Accept-Language header → default (Az). No cookie/session state, so the
/// same request always resolves the same language — simple and stateless.
/// </summary>
public class LanguageProvider(IHttpContextAccessor httpContextAccessor) : ILanguageProvider
{
    public Language Current
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext is null)
            {
                return Language.Az;
            }

            if (httpContext.Request.Query.TryGetValue("lang", out var queryLang) &&
                TryParse(queryLang.ToString(), out var fromQuery))
            {
                return fromQuery;
            }

            var header = httpContext.Request.Headers.AcceptLanguage.ToString();
            if (!string.IsNullOrWhiteSpace(header))
            {
                var primary = header.Split(',').FirstOrDefault()?.Split(';').FirstOrDefault()?.Split('-').FirstOrDefault();
                if (primary is not null && TryParse(primary, out var fromHeader))
                {
                    return fromHeader;
                }
            }

            return Language.Az;
        }
    }

    private static bool TryParse(string? value, out Language language)
    {
        language = value?.Trim().ToLowerInvariant() switch
        {
            "az" => Language.Az,
            "en" => Language.En,
            "ru" => Language.Ru,
            _ => Language.Az
        };
        return value is not null && (value.Equals("az", StringComparison.OrdinalIgnoreCase) ||
                                      value.Equals("en", StringComparison.OrdinalIgnoreCase) ||
                                      value.Equals("ru", StringComparison.OrdinalIgnoreCase));
    }
}
