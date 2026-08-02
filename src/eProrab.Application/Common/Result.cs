namespace eProrab.Application.Common;

/// <summary>Non-generic result for operations that don't return a value (e.g. Delete).</summary>
public readonly record struct Result(bool IsSuccess, string? Error, string? ErrorCode)
{
    public static Result Ok() => new(true, null, null);

    public static Result Fail(string error, string errorCode = "error") => new(false, error, errorCode);
}

/// <summary>Generic result carrying a value on success or an error message/code on failure.</summary>
public readonly record struct Result<T>(T? Value, string? Error, string? ErrorCode, bool IsSuccess)
{
    public static Result<T> Ok(T value) => new(value, null, null, true);

    public static Result<T> Fail(string error, string errorCode = "error") => new(default, error, errorCode, false);
}
