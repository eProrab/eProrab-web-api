namespace eProrab.Domain.Constants;

/// <summary>
/// Fixed role names seeded into ASP.NET Core Identity at startup.
/// Kept as constants (not an enum) because Identity roles are string-based
/// and referenced by name in [Authorize(Roles = "...")] and policies.
/// </summary>
public static class Roles
{
    /// <summary>Full administrative access: manages the item catalog, categories and users.</summary>
    public const string Admin = "Admin";

    /// <summary>Site foreman ("prorab") — manages jobs/orders, reads the catalog, limited write access.</summary>
    public const string Manager = "Manager";

    /// <summary>Regular customer/client account — buys materials and/or posts jobs.</summary>
    public const string Client = "Client";

    /// <summary>Job-seeking tradesperson (mason, electrician, painter, etc.) with a worker cabinet.</summary>
    public const string Worker = "Worker";

    /// <summary>Architect / Designer performing author supervision and project design.</summary>
    public const string Architect = "Architect";

    /// <summary>Construction & repair materials market / store partner.</summary>
    public const string Market = "Market";

    public static readonly string[] All = [Admin, Manager, Client, Worker, Architect, Market];
}

