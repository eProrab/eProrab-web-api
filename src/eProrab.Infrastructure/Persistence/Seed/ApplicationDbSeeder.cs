using eProrab.Domain.Constants;
using eProrab.Domain.Entities;
using eProrab.Domain.Enums;
using eProrab.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eProrab.Infrastructure.Persistence.Seed;

/// <summary>
/// Applies pending migrations and seeds roles, a first Admin account, and a
/// starter trilingual catalog (categories, specializations, a few items) so
/// the API is immediately usable after `dotnet run` against an empty database.
/// </summary>
public static class ApplicationDbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        var context = provider.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(provider);
        await SeedAdminAsync(provider, logger);
        await SeedCategoriesAsync(context);
        await SeedSpecializationsAsync(context);
        await ClearItemsAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(IServiceProvider provider)
    {
        var roleManager = provider.GetRequiredService<RoleManager<ApplicationRole>>();
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }
    }

    private static async Task SeedAdminAsync(IServiceProvider provider, ILogger logger)
    {
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = provider.GetRequiredService<IConfiguration>();

        var email = configuration["SeedAdmin:Email"] ?? "admin@eprorab.az";
        var password = configuration["SeedAdmin:Password"] ?? "ChangeMe123!";

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = "eProrab Admin",
            PreferredLanguage = Language.Az,
            IsActive = true
        };

        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, Roles.Admin);
            logger.LogWarning("Seeded default admin account {Email}. Change its password immediately outside development.", email);
        }
        else
        {
            logger.LogError("Failed to seed admin account: {Errors}", string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new[]
        {
            Category("cement-concrete", 1, ("Sement və Beton", "Cement & Concrete", "Цемент и бетон")),
            Category("hand-tools", 2, ("Əl alətləri", "Hand Tools", "Ручной инструмент")),
            Category("electrical", 3, ("Elektrik materialları", "Electrical", "Электрика")),
            Category("plumbing", 4, ("Santexnika", "Plumbing", "Сантехника")),
            Category("paint-finishing", 5, ("Boya və bəzək", "Paint & Finishing", "Краска и отделка"))
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
        return;

        Category Category(string slug, int order, (string Az, string En, string Ru) names) => new()
        {
            Slug = slug,
            DisplayOrder = order,
            IsActive = true,
            Translations =
            [
                new CategoryTranslation { Language = Language.Az, Name = names.Az },
                new CategoryTranslation { Language = Language.En, Name = names.En },
                new CategoryTranslation { Language = Language.Ru, Name = names.Ru }
            ]
        };
    }

    private static async Task SeedSpecializationsAsync(ApplicationDbContext context)
    {
        if (await context.Specializations.AnyAsync())
        {
            return;
        }

        var specializations = new[]
        {
            Specialization("mason", 1, ("Bənna", "Mason", "Каменщик")),
            Specialization("electrician", 2, ("Elektrik", "Electrician", "Электрик")),
            Specialization("plumber", 3, ("Santexnik", "Plumber", "Сантехник")),
            Specialization("painter", 4, ("Rəngsaz", "Painter", "Маляр")),
            Specialization("carpenter", 5, ("Dülgər", "Carpenter", "Плотник")),
            Specialization("crane-operator", 6, ("Kran operatoru", "Crane Operator", "Крановщик"))
        };

        context.Specializations.AddRange(specializations);
        await context.SaveChangesAsync();
        return;

        Specialization Specialization(string slug, int order, (string Az, string En, string Ru) names) => new()
        {
            Slug = slug,
            DisplayOrder = order,
            IsActive = true,
            Translations =
            [
                new SpecializationTranslation { Language = Language.Az, Name = names.Az },
                new SpecializationTranslation { Language = Language.En, Name = names.En },
                new SpecializationTranslation { Language = Language.Ru, Name = names.Ru }
            ]
        };
    }

    private static async Task ClearItemsAsync(ApplicationDbContext context)
    {
        if (await context.Items.AnyAsync())
        {
            context.Items.RemoveRange(context.Items);
            await context.SaveChangesAsync();
        }
    }
}
