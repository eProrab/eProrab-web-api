using eProrab.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eProrab.Infrastructure.Persistence.Configurations;

public class SavedCalculationConfiguration : IEntityTypeConfiguration<SavedCalculation>
{
    public void Configure(EntityTypeBuilder<SavedCalculation> builder)
    {
        builder.ToTable("SavedCalculations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.PropertyType)
            .HasMaxLength(50);

        builder.Property(c => c.StructureAge)
            .HasMaxLength(50);

        builder.Property(c => c.RepairStyle)
            .HasMaxLength(50);

        builder.Property(c => c.TariffTier)
            .HasMaxLength(50);

        builder.Property(c => c.TotalBudget)
            .HasPrecision(18, 2);

        builder.Property(c => c.MaterialCost)
            .HasPrecision(18, 2);

        builder.Property(c => c.LaborCost)
            .HasPrecision(18, 2);

        builder.Property(c => c.OtherCost)
            .HasPrecision(18, 2);

        builder.Property(c => c.RoomsJson)
            .HasColumnType("text");

        builder.HasIndex(c => c.UserId);
    }
}
