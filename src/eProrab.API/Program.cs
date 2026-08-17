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

var builder = WebApplication.CreateBuilder(args);

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
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

const string CorsPolicy = "eProrabCors";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
        else
        {
            // No origins configured (e.g. first local run) — permissive default so the API is usable out of the box.
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// ---------------------------------------------------------------- pipeline

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
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
//parser
app.MapMaterialPricesEndpoints();

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

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program;
