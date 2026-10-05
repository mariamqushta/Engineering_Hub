using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Engineering_Hub.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonContentAndFinalizeDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrackEnrollments_StudentId",
                table: "TrackEnrollments");

            migrationBuilder.DropColumn(
                name: "ContentUrl",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "LessonType",
                table: "Lessons");

            migrationBuilder.CreateTable(
                name: "LessonTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LessonContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    LessonTypeId = table.Column<int>(type: "int", nullable: false),
                    ContentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StoredFileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    IsFree = table.Column<bool>(type: "bit", nullable: true),
                    WorkshopId = table.Column<int>(type: "int", nullable: true),
                    InteractiveActivityId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonContents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonContents_InteractiveActivities_InteractiveActivityId",
                        column: x => x.InteractiveActivityId,
                        principalTable: "InteractiveActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonContents_LessonTypes_LessonTypeId",
                        column: x => x.LessonTypeId,
                        principalTable: "LessonTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonContents_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonContents_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "LessonTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Video" },
                    { 2, "PDF" },
                    { 3, "PowerPoint" },
                    { 4, "Interactive" },
                    { 5, "Workshop" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackEnrollments_StudentId_TrackId",
                table: "TrackEnrollments",
                columns: new[] { "StudentId", "TrackId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonContents_InteractiveActivityId",
                table: "LessonContents",
                column: "InteractiveActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonContents_LessonId",
                table: "LessonContents",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonContents_LessonTypeId",
                table: "LessonContents",
                column: "LessonTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonContents_WorkshopId",
                table: "LessonContents",
                column: "WorkshopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonContents");

            migrationBuilder.DropTable(
                name: "LessonTypes");

            migrationBuilder.DropIndex(
                name: "IX_TrackEnrollments_StudentId_TrackId",
                table: "TrackEnrollments");

            migrationBuilder.AddColumn<string>(
                name: "ContentUrl",
                table: "Lessons",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LessonType",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrackEnrollments_StudentId",
                table: "TrackEnrollments",
                column: "StudentId");
        }
    }
}
