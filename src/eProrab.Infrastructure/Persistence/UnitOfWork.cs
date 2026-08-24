using eProrab.Application.Interfaces;
using eProrab.Domain.Entities;

namespace eProrab.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Categories = new Repository<Category>(context);
        CategoryTranslations = new Repository<CategoryTranslation>(context);
        Items = new Repository<Item>(context);
        ItemTranslations = new Repository<ItemTranslation>(context);
        Specializations = new Repository<Specialization>(context);
        SpecializationTranslations = new Repository<SpecializationTranslation>(context);
        WorkerProfiles = new Repository<WorkerProfile>(context);
        JobPostings = new Repository<JobPosting>(context);
        JobApplications = new Repository<JobApplication>(context);
        RefreshTokens = new Repository<RefreshToken>(context);
        SavedCalculations = new Repository<SavedCalculation>(context);
        DirectMessages = new Repository<DirectMessage>(context);
    }

    public IRepository<Category> Categories { get; }
    public IRepository<CategoryTranslation> CategoryTranslations { get; }
    public IRepository<Item> Items { get; }
    public IRepository<ItemTranslation> ItemTranslations { get; }
    public IRepository<Specialization> Specializations { get; }
    public IRepository<SpecializationTranslation> SpecializationTranslations { get; }
    public IRepository<WorkerProfile> WorkerProfiles { get; }
    public IRepository<JobPosting> JobPostings { get; }
    public IRepository<JobApplication> JobApplications { get; }
    public IRepository<RefreshToken> RefreshTokens { get; }
    public IRepository<SavedCalculation> SavedCalculations { get; }
    public IRepository<DirectMessage> DirectMessages { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
