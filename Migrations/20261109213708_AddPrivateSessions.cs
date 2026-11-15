using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engineering_Hub.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountType",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PrivateArrangements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InstructorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    TrackId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateArrangements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivateArrangements_AspNetUsers_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateArrangements_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateArrangements_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrivateAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstructorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivateAvailabilities_AspNetUsers_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrivateOccurrences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrivateArrangementId = table.Column<int>(type: "int", nullable: true),
                    StudentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    InstructorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TrackId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateOccurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivateOccurrences_AspNetUsers_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateOccurrences_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateOccurrences_PrivateArrangements_PrivateArrangementId",
                        column: x => x.PrivateArrangementId,
                        principalTable: "PrivateArrangements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateOccurrences_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrivateReplacements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MissedOccurrenceId = table.Column<int>(type: "int", nullable: false),
                    ReplacementOccurrenceId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedByAdminId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateReplacements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivateReplacements_PrivateOccurrences_MissedOccurrenceId",
                        column: x => x.MissedOccurrenceId,
                        principalTable: "PrivateOccurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateReplacements_PrivateOccurrences_ReplacementOccurrenceId",
                        column: x => x.ReplacementOccurrenceId,
                        principalTable: "PrivateOccurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrivateRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurrenceId = table.Column<int>(type: "int", nullable: false),
                    RequestType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivateRequests_PrivateOccurrences_OccurrenceId",
                        column: x => x.OccurrenceId,
                        principalTable: "PrivateOccurrences",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrivateArrangements_InstructorId",
                table: "PrivateArrangements",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateArrangements_StudentId",
                table: "PrivateArrangements",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateArrangements_TrackId",
                table: "PrivateArrangements",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateAvailabilities_InstructorId_DayOfWeek",
                table: "PrivateAvailabilities",
                columns: new[] { "InstructorId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_PrivateOccurrences_InstructorId",
                table: "PrivateOccurrences",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateOccurrences_PrivateArrangementId",
                table: "PrivateOccurrences",
                column: "PrivateArrangementId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateOccurrences_StartUtc",
                table: "PrivateOccurrences",
                column: "StartUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateOccurrences_StudentId",
                table: "PrivateOccurrences",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateOccurrences_TrackId",
                table: "PrivateOccurrences",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateReplacements_MissedOccurrenceId",
                table: "PrivateReplacements",
                column: "MissedOccurrenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrivateReplacements_ReplacementOccurrenceId",
                table: "PrivateReplacements",
                column: "ReplacementOccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateRequests_OccurrenceId",
                table: "PrivateRequests",
                column: "OccurrenceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrivateAvailabilities");

            migrationBuilder.DropTable(
                name: "PrivateReplacements");

            migrationBuilder.DropTable(
                name: "PrivateRequests");

            migrationBuilder.DropTable(
                name: "PrivateOccurrences");

            migrationBuilder.DropTable(
                name: "PrivateArrangements");

            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "AspNetUsers");
        }
    }
}
