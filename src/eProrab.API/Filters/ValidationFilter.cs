using FluentValidation;

namespace eProrab.API.Filters;

/// <summary>
/// Validates the request DTO of type <typeparamref name="T"/> using its registered
/// FluentValidation validator before the endpoint delegate runs. Throws
/// <see cref="ValidationException"/> on failure, which <see cref="Middleware.GlobalExceptionHandler"/>
/// turns into a 400 ProblemDetails response with field-level errors.
/// Attach with <c>.AddEndpointFilter(new ValidationFilter&lt;TRequest&gt;())</c>.
/// </summary>
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is null)
        {
            return await next(context);
        }

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null)
        {
            return await next(context);
        }

        var result = await validator.ValidateAsync(argument);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }

        return await next(context);
    }
}
