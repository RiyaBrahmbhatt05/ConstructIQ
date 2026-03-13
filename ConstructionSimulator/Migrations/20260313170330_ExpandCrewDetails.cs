using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionSimulator.Migrations
{
    /// <inheritdoc />
    public partial class ExpandCrewDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactDetails",
                table: "Crews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MemberNames",
                table: "Crews",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredLicenses",
                table: "Crews",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                table: "Crews",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactDetails",
                table: "Crews");

            migrationBuilder.DropColumn(
                name: "MemberNames",
                table: "Crews");

            migrationBuilder.DropColumn(
                name: "RequiredLicenses",
                table: "Crews");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                table: "Crews");
        }
    }
}
