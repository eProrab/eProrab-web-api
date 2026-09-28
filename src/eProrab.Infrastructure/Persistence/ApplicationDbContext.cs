using System.Reflection;
using eProrab.Domain.Common;
using eProrab.Domain.Entities;
using eProrab.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace eProrab.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryTranslation> CategoryTranslations => Set<CategoryTranslation>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemTranslation> ItemTranslations => Set<ItemTranslation>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<SpecializationTranslation> SpecializationTranslations => Set<SpecializationTranslation>();
    public DbSet<WorkerProfile> WorkerProfiles => Set<WorkerProfile>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SavedCalculation> SavedCalculations => Set<SavedCalculation>();
    public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();
    public DbSet<MarketProfile> MarketProfiles => Set<MarketProfile>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Shrink the sprawling default Identity table names down to something readable.
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        // Global soft-delete filter for every entity that derives from BaseEntity,
        // applied once via reflection instead of repeating HasQueryFilter everywhere.
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
            var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
            var condition = System.Linq.Expressions.Expression.Lambda(
                System.Linq.Expressions.Expression.Not(property), parameter);

            builder.Entity(entityType.ClrType).HasQueryFilter(condition);
        }
    }
}
