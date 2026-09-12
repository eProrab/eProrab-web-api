using eProrab.API.Endpoints;
using eProrab.API.Extensions;
using eProrab.API.Middleware;
using eProrab.Application;
using eProrab.Infrastructure;
using eProrab.Infrastructure.Options;
using eProrab.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Load .env file if present (e.g. local development)
LoadDotEnv();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory()
});

// If the container runtime provides a PORT environment variable (e.g. Render),
// configure the app to listen on that port at runtime.
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(portEnv))
{
    builder.WebHost.UseUrls($"http://*:{portEnv}");
}

// ---------------------------------------------------------------- services

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAppAuthorization();
builder.Services.AddSwaggerWithJwt();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(10)
        };
    });

const string CorsPolicy = "eProrabCors";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
var isDevelopment = builder.Environment.IsDevelopment();
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else if (isDevelopment)
        {
            // Development: permissive for local testing
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            // Production: restrict to prevent CORS abuse
            policy.WithOrigins("https://example.com").AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// ---------------------------------------------------------------- pipeline

app.UseExceptionHandler();

// Add security headers to all responses
app.UseSecurityHeaders();

// Enable Swagger in Development, or when ENABLE_SWAGGER=true is set in the environment.
var enableSwaggerEnv = Environment.GetEnvironmentVariable("ENABLE_SWAGGER");
if (app.Environment.IsDevelopment() || string.Equals(enableSwaggerEnv, "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "eProrab API v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

// ---------------------------------------------------------------- endpoints

app.MapAuthEndpoints();
app.MapCatalogEndpoints();
app.MapAdminCatalogEndpoints();
app.MapAdminUsersEndpoints();
app.MapWorkerCabinetEndpoints();
app.MapWorkerBrowseEndpoints();
app.MapJobEndpoints();
app.MapAdminHiringEndpoints();
app.MapCalculationEndpoints();
app.MapChatEndpoints();
app.MapMarketCabinetEndpoints();


app.MapGet("/", () => Results.Ok(new { service = "eProrab API", status = "running" }))
    .ExcludeFromDescription();

// ---------------------------------------------------------------- startup: migrate + seed

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        await ApplicationDbSeeder.SeedAsync(app.Services);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration/seed failed at startup. " +
                             "Check the 'Default' connection string and that PostgreSQL is reachable.");
    }
}

app.Run();

static void LoadDotEnv()
{
    var currentDir = Directory.GetCurrentDirectory();
    var possiblePaths = new[]
    {
        Path.Combine(currentDir, ".env"),
        Path.Combine(currentDir, "..", ".env"),
        Path.Combine(currentDir, "..", "..", ".env")
    };

    foreach (var path in possiblePaths)
    {
        if (File.Exists(path))
        {
            foreach (var line in File.ReadAllLines(path))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                    continue;

                var separatorIndex = trimmed.IndexOf('=');
                if (separatorIndex <= 0)
                    continue;

                var key = trimmed[..separatorIndex].Trim();
                var value = trimmed[(separatorIndex + 1)..].Trim();

                if (Environment.GetEnvironmentVariable(key) is null)
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
            break;
        }
    }
}

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program;
