using eProrab.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProrab.Infrastructure.Persistence.Configurations;

public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.Property(j => j.Title).IsRequired().HasMaxLength(150);
        builder.Property(j => j.Description).IsRequired().HasMaxLength(4000);
        builder.Property(j => j.City).HasMaxLength(100);
        builder.Property(j => j.BudgetMin).HasPrecision(12, 2);
        builder.Property(j => j.BudgetMax).HasPrecision(12, 2);

        builder.HasMany(j => j.Applications)
            .WithOne(a => a.JobPosting)
            .HasForeignKey(a => a.JobPostingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(j => j.PostedByUserId);
        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.SpecializationId);
    }
}
