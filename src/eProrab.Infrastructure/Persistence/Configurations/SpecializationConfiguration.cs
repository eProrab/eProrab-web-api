using eProrab.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProrab.Infrastructure.Persistence.Configurations;

public class SpecializationConfiguration : IEntityTypeConfiguration<Specialization>
{
    public void Configure(EntityTypeBuilder<Specialization> builder)
    {
        builder.Property(s => s.Slug).IsRequired().HasMaxLength(80);
        builder.HasIndex(s => s.Slug).IsUnique();

        builder.HasMany(s => s.Translations)
            .WithOne(t => t.Specialization)
            .HasForeignKey(t => t.SpecializationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.WorkerProfiles)
            .WithOne(w => w.Specialization)
            .HasForeignKey(w => w.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.JobPostings)
            .WithOne(j => j.Specialization)
            .HasForeignKey(j => j.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SpecializationTranslationConfiguration : IEntityTypeConfiguration<SpecializationTranslation>
{
    public void Configure(EntityTypeBuilder<SpecializationTranslation> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
        builder.HasIndex(t => new { t.SpecializationId, t.Language }).IsUnique();
    }
}
