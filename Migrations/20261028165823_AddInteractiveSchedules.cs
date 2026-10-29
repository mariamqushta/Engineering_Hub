using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engineering_Hub.Migrations
{
    /// <inheritdoc />
    public partial class AddInteractiveSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InteractiveScheduleId",
                table: "InteractiveActivities",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InteractiveSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    InstructorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    MeetingUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DaysOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractiveSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractiveSchedules_AspNetUsers_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InteractiveSchedules_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveActivities_InteractiveScheduleId",
                table: "InteractiveActivities",
                column: "InteractiveScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveSchedules_InstructorId",
                table: "InteractiveSchedules",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveSchedules_TrackId",
                table: "InteractiveSchedules",
                column: "TrackId");

            migrationBuilder.AddForeignKey(
                name: "FK_InteractiveActivities_InteractiveSchedules_InteractiveScheduleId",
                table: "InteractiveActivities",
                column: "InteractiveScheduleId",
                principalTable: "InteractiveSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InteractiveActivities_InteractiveSchedules_InteractiveScheduleId",
                table: "InteractiveActivities");

            migrationBuilder.DropTable(
                name: "InteractiveSchedules");

            migrationBuilder.DropIndex(
                name: "IX_InteractiveActivities_InteractiveScheduleId",
                table: "InteractiveActivities");

            migrationBuilder.DropColumn(
                name: "InteractiveScheduleId",
                table: "InteractiveActivities");
        }
    }
}
