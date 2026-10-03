using eProrab.Application.Common;
using eProrab.Application.Interfaces;
using eProrab.Application.Localization;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eProrab.API.Middleware;

/// <summary>
/// Central place every unhandled exception funnels through. Keeps controllers/
/// endpoints free of try/catch: they just throw <see cref="AppException"/>
/// subtypes (or FluentValidation throws its own) and this maps them to the
/// right HTTP status + a consistent ProblemDetails body.
/// </summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
            TooManyRequestsException => (StatusCodes.Status429TooManyRequests, "Too Many Requests"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected Error")
        };

        var detail = exception.Message;

        if (exception is DbUpdateException dbEx &&
            (dbEx.InnerException?.Message.Contains("IX_Users_PhoneNumber", StringComparison.OrdinalIgnoreCase) == true ||
             dbEx.Message.Contains("IX_Users_PhoneNumber", StringComparison.OrdinalIgnoreCase)))
        {
            var langProvider = httpContext.RequestServices.GetService<ILanguageProvider>();
            var lang = langProvider?.Current ?? eProrab.Domain.Enums.Language.Az;
            detail = Messages.Get(SystemMessageKey.PhoneNumberAlreadyRegistered, lang);
            statusCode = StatusCodes.Status409Conflict;
            title = "Conflict";
        }

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception processing {Path}", httpContext.Request.Path);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        if (exception is TooManyRequestsException tooManyRequestsException)
        {
            if (tooManyRequestsException.RetryAfterSeconds.HasValue)
            {
                httpContext.Response.Headers.RetryAfter = tooManyRequestsException.RetryAfterSeconds.Value.ToString();
                problemDetails.Extensions["retryAfterSeconds"] = tooManyRequestsException.RetryAfterSeconds.Value;
            }
        }

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
