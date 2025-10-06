using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HabitTrackerApp.Migrations
{
    /// <inheritdoc />
    public partial class AddExercisesAndExerciseLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HabitType",
                table: "Habits",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HabitId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    OrderIndex = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    VideoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LocalVideoPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DocumentUrls = table.Column<string>(type: "TEXT", nullable: true),
                    TargetSets = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetReps = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetWeight = table.Column<double>(type: "REAL", nullable: true),
                    TargetDuration = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetRPE = table.Column<int>(type: "INTEGER", nullable: true),
                    RestSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    ExerciseType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    MuscleGroups = table.Column<string>(type: "TEXT", nullable: true),
                    Equipment = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SyncStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercises_Habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "Habits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ExerciseId = table.Column<int>(type: "INTEGER", nullable: false),
                    DailyHabitEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SetNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    ActualReps = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualWeight = table.Column<double>(type: "REAL", nullable: true),
                    ActualDuration = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualRPE = table.Column<int>(type: "INTEGER", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DeviceId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SyncStatus = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseLogs_DailyHabitEntries_DailyHabitEntryId",
                        column: x => x.DailyHabitEntryId,
                        principalTable: "DailyHabitEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseLogs_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseLogs_DailyHabitEntryId",
                table: "ExerciseLogs",
                column: "DailyHabitEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseLogs_Date",
                table: "ExerciseLogs",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseLogs_ExerciseId",
                table: "ExerciseLogs",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_HabitId",
                table: "Exercises",
                column: "HabitId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_OrderIndex",
                table: "Exercises",
                column: "OrderIndex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseLogs");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropColumn(
                name: "HabitType",
                table: "Habits");
        }
    }
}
