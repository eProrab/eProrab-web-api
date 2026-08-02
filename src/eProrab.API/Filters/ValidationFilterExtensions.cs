namespace eProrab.API.Filters;

public static class ValidationFilterExtensions
{
    /// <summary>Runs FluentValidation on the endpoint's <typeparamref name="T"/> body parameter before the handler.</summary>
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class =>
        builder.AddEndpointFilter(new ValidationFilter<T>());
}
