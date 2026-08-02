using eProrab.Domain.Constants;

namespace eProrab.API.Extensions;

/// <summary>Named authorization policies used across endpoint groups.</summary>
public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string EmployerRoles = "EmployerRoles"; // can post/manage jobs
    public const string WorkerOnly = "WorkerOnly";

    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AdminOnly, p => p.RequireRole(Roles.Admin))
            .AddPolicy(EmployerRoles, p => p.RequireRole(Roles.Client, Roles.Manager, Roles.Admin))
            .AddPolicy(WorkerOnly, p => p.RequireRole(Roles.Worker));

        return services;
    }
}
