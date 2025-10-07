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
            .HasMaxLength(LengthConstants.LENGTH100)
            .HasColumnName("position_name")
            .HasConversion(p => p.Value, p => PositionName.Create(p).Value);

        builder.Property(p => p.Description)
            .IsRequired(false)
            .HasMaxLength(LengthConstants.LENGTH1000)
            .HasColumnName("description");

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(p => p.UpdatedAt)
            .IsRequired(false)
            .HasColumnName("updated_at");
    }
}