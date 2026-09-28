using eProrab.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProrab.Infrastructure.Persistence.Configurations;

public class MarketProfileConfiguration : IEntityTypeConfiguration<MarketProfile>
{
    public void Configure(EntityTypeBuilder<MarketProfile> builder)
    {
        builder.Property(m => m.StoreName).IsRequired().HasMaxLength(200);
        builder.Property(m => m.Voen).HasMaxLength(50);
        builder.Property(m => m.ContactPhone).HasMaxLength(50);
        builder.Property(m => m.ContactEmail).HasMaxLength(100);
        builder.Property(m => m.Address).HasMaxLength(300);
        builder.Property(m => m.City).HasMaxLength(100);
        builder.Property(m => m.Description).HasMaxLength(2000);
        builder.Property(m => m.WorkingHours).HasMaxLength(200);

        builder.HasIndex(m => m.UserId).IsUnique();
    }
}
