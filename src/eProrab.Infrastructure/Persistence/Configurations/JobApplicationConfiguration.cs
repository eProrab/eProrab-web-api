using eProrab.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProrab.Infrastructure.Persistence.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.Property(a => a.CoverMessage).HasMaxLength(2000);
        builder.Property(a => a.ProposedRate).HasPrecision(10, 2);
        builder.Property(a => a.AgreedRate).HasPrecision(10, 2);

        // A worker can have at most one *active* (non-withdrawn/rejected) application per job —
        // enforced in the service layer (partial-unique-index semantics vary by provider),
        // but we still index the pair for fast lookups.
        builder.HasIndex(a => new { a.JobPostingId, a.WorkerProfileId });
        builder.HasIndex(a => a.Status);
    }
}
