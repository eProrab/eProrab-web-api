using eProrab.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProrab.Infrastructure.Persistence.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.Property(i => i.Sku).IsRequired().HasMaxLength(50);
        builder.HasIndex(i => i.Sku).IsUnique();

        builder.Property(i => i.Price).HasPrecision(12, 2);
        builder.Property(i => i.StockQuantity).HasPrecision(12, 3);
        builder.Property(i => i.ImageUrl).HasMaxLength(2048);
        builder.Property(i => i.Dimensions).HasMaxLength(200);
        builder.Property(i => i.MarketName).HasMaxLength(200);

        builder.HasMany(i => i.Translations)
            .WithOne(t => t.Item)
            .HasForeignKey(t => t.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.CategoryId);
        builder.HasIndex(i => i.IsActive);
        builder.HasIndex(i => i.MarketUserId);
    }
}

public class ItemTranslationConfiguration : IEntityTypeConfiguration<ItemTranslation>
{
    public void Configure(EntityTypeBuilder<ItemTranslation> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Description).HasMaxLength(4000);
        builder.HasIndex(t => new { t.ItemId, t.Language }).IsUnique();
    }
}
