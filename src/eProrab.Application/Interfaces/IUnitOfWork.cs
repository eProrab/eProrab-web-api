using eProrab.Domain.Entities;

namespace eProrab.Application.Interfaces;

/// <summary>
/// Aggregates all repositories behind a single unit of work so a service can
/// perform multi-entity operations (e.g. create a Category + its 3 translations)
/// and persist them atomically with one <see cref="SaveChangesAsync"/> call.
/// </summary>
public interface IUnitOfWork
{
    IRepository<Category> Categories { get; }
    IRepository<CategoryTranslation> CategoryTranslations { get; }
    IRepository<Item> Items { get; }
    IRepository<ItemTranslation> ItemTranslations { get; }
    IRepository<Specialization> Specializations { get; }
    IRepository<SpecializationTranslation> SpecializationTranslations { get; }
    IRepository<WorkerProfile> WorkerProfiles { get; }
    IRepository<JobPosting> JobPostings { get; }
    IRepository<JobApplication> JobApplications { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    IRepository<SavedCalculation> SavedCalculations { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
