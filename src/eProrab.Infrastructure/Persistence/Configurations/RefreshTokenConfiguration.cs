using eProrab.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProrab.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(t => t.Token).IsRequired().HasMaxLength(200);
        builder.HasIndex(t => t.Token).IsUnique();
        builder.HasIndex(t => t.UserId);
    }
}
