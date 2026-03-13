using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionSimulator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMaterialUnitType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "Materials");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "Materials",
                newName: "UnitType");

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "TaskMaterials",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Materials",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "TaskMaterials");

            migrationBuilder.RenameColumn(
                name: "UnitType",
                table: "Materials",
                newName: "Unit");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Materials",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Supplier",
                table: "Materials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
