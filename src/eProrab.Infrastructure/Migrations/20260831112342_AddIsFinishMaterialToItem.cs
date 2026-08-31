using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eProrab.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFinishMaterialToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only add the new column; all other tables already exist in the DB.
            migrationBuilder.AddColumn<bool>(
                name: "IsFinishMaterial",
                table: "Items",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFinishMaterial",
                table: "Items");
        }
    }
}
