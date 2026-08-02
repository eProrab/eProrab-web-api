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
        await SeedItemsAsync(context);

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

    private static async Task SeedItemsAsync(ApplicationDbContext context)
    {
        if (await context.Items.AnyAsync())
        {
            return;
        }

        var cement = await context.Categories.FirstAsync(c => c.Slug == "cement-concrete");
        var tools = await context.Categories.FirstAsync(c => c.Slug == "hand-tools");

        context.Items.AddRange(
            new Item
            {
                Sku = "CEM-50KG",
                CategoryId = cement.Id,
                Unit = UnitOfMeasure.Bag,
                Price = 9.50m,
                StockQuantity = 500,
                IsActive = true,
                Translations =
                [
                    new ItemTranslation { Language = Language.Az, Name = "Portland sementi 50kg", Description = "Ümumi tikinti işləri üçün M400 sement." },
                    new ItemTranslation { Language = Language.En, Name = "Portland Cement 50kg", Description = "M400-grade cement for general construction work." },
                    new ItemTranslation { Language = Language.Ru, Name = "Портландцемент 50кг", Description = "Цемент марки М400 для общестроительных работ." }
                ]
            },
            new Item
            {
                Sku = "HMR-STD",
                CategoryId = tools.Id,
                Unit = UnitOfMeasure.Piece,
                Price = 12.90m,
                StockQuantity = 120,
                IsActive = true,
                Translations =
                [
                    new ItemTranslation { Language = Language.Az, Name = "Bənna çəkici", Description = "Standart tikinti çəkici, poladdan hazırlanıb." },
                    new ItemTranslation { Language = Language.En, Name = "Mason's Hammer", Description = "Standard steel construction hammer." },
                    new ItemTranslation { Language = Language.Ru, Name = "Молоток каменщика", Description = "Стандартный строительный молоток из стали." }
                ]
            });

        await context.SaveChangesAsync();
    }
}
