using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConstructionSimulator.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaskMaterials",
                columns: table => new
                {
                    TaskMaterialId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectTaskId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    UnitsRequired = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubtotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskMaterials", x => x.TaskMaterialId);
                    table.ForeignKey(
                        name: "FK_TaskMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "MaterialId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskMaterials_Tasks_ProjectTaskId",
                        column: x => x.ProjectTaskId,
                        principalTable: "Tasks",
                        principalColumn: "ProjectTaskId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaskMaterials_MaterialId",
                table: "TaskMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskMaterials_ProjectTaskId",
                table: "TaskMaterials",
                column: "ProjectTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskMaterials");
        }
    }
}
