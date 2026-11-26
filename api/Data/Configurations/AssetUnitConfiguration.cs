using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SitesLend.Api.Entities;

namespace SitesLend.Api.Data.Configurations;

public class AssetUnitConfiguration : IEntityTypeConfiguration<AssetUnit>
{
    public void Configure(EntityTypeBuilder<AssetUnit> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.AssetTag).IsRequired().HasMaxLength(30);
        builder.HasIndex(u => u.AssetTag).IsUnique();

        builder.Property(u => u.Description).HasMaxLength(500);

        // Enums are stored as readable text, not numbers.
        builder.Property(u => u.Condition).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(u => u.State).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(u => u.CreatedAt).IsRequired();

        // Npgsql maps a uint row version to the xmin system column, so no column is created.
        builder.Property(u => u.Version).IsRowVersion();

        builder.HasOne(u => u.Category)
            .WithMany(c => c.Units)
            .HasForeignKey(u => u.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
