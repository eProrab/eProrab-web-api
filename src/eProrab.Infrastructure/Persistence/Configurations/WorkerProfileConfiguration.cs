using eProrab.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProrab.Infrastructure.Persistence.Configurations;

public class WorkerProfileConfiguration : IEntityTypeConfiguration<WorkerProfile>
{
    public void Configure(EntityTypeBuilder<WorkerProfile> builder)
    {
        // One worker profile per user account.
        builder.HasIndex(w => w.UserId).IsUnique();

        builder.Property(w => w.CompanyName).HasMaxLength(200);
        builder.Property(w => w.Voen).HasMaxLength(50);
        builder.Property(w => w.Bio).HasMaxLength(2000);
        builder.Property(w => w.City).HasMaxLength(100);
        builder.Property(w => w.DailyRate).HasPrecision(10, 2);

        builder.HasMany(w => w.Applications)
            .WithOne(a => a.WorkerProfile)
            .HasForeignKey(a => a.WorkerProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

