using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eProrab.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSurfaceTypeToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SurfaceType",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SurfaceType",
                table: "Items");
        }
    }
}
