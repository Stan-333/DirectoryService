using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeNullableLocationAddressUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS idx_location_address;

                CREATE UNIQUE INDEX idx_location_address
                ON locations (
                    postal_code,
                    region,
                    city,
                    street,
                    house,
                    (COALESCE(apartment, '')))
                WHERE is_active = true;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS idx_location_address;

                CREATE UNIQUE INDEX idx_location_address
                ON locations (postal_code, region, city, street, house, apartment)
                WHERE is_active = true;
                """);
        }
    }
}