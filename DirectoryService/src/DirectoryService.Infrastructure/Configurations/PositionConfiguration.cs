using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");

        builder.HasKey(p => p.Id).HasName("pk_position");

        builder.Property(p => p.Id)
            .HasConversion(p => p.Value, p => new PositionId(p))
            .HasColumnName("position_id");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(PositionName.NAME_MAX_LENGTH)
            .HasColumnName("position_name")
            .HasConversion(p => p.Value, p => PositionName.Create(p).Value);

        builder.HasIndex(p => p.Name)
            .HasDatabaseName("idx_position_name")
            .HasFilter("is_active = true")
            .IsUnique();

        builder.Property(p => p.Description)
            .IsRequired(false)
            .HasMaxLength(Position.DESCRIPTION_MAX_LENGTH)
            .HasColumnName("description");

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasColumnName("updated_at");

        builder.HasMany(p => p.DepartmentPositions)
            .WithOne(dp => dp.Position)
            .HasForeignKey(dp => dp.PositionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}