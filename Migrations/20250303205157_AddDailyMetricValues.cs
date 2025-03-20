using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTrackerApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyMetricValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyMetricValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyHabitEntryId = table.Column<int>(type: "int", nullable: false),
                    HabitMetricDefinitionId = table.Column<int>(type: "int", nullable: false),
                    NumericValue = table.Column<double>(type: "float", nullable: true),
                    TextValue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyMetricValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyMetricValues_DailyHabitEntries_DailyHabitEntryId",
                        column: x => x.DailyHabitEntryId,
                        principalTable: "DailyHabitEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DailyMetricValues_HabitMetricDefinitions_HabitMetricDefinitionId",
                        column: x => x.HabitMetricDefinitionId,
                        principalTable: "HabitMetricDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyMetricValues_DailyHabitEntryId",
                table: "DailyMetricValues",
                column: "DailyHabitEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyMetricValues_HabitMetricDefinitionId",
                table: "DailyMetricValues",
                column: "HabitMetricDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyMetricValues");
        }
    }
}
