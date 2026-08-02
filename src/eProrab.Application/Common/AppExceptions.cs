namespace eProrab.Application.Common;

/// <summary>Base type for exceptions the global exception middleware maps to a specific HTTP status.</summary>
public abstract class AppException(string message) : Exception(message);

/// <summary>Requested resource does not exist → 404.</summary>
public sealed class NotFoundException(string resource, object key)
    : AppException($"{resource} with id '{key}' was not found.");

/// <summary>Request conflicts with current state (duplicate SKU, email already used, etc.) → 409.</summary>
public sealed class ConflictException(string message) : AppException(message);

/// <summary>Caller is authenticated but not allowed to perform this action → 403.</summary>
public sealed class ForbiddenException(string message = "You are not allowed to perform this action.")
    : AppException(message);
