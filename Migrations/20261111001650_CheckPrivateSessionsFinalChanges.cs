using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engineering_Hub.Migrations
{
    /// <inheritdoc />
    public partial class CheckPrivateSessionsFinalChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PrivateRequests_OccurrenceId",
                table: "PrivateRequests");

            migrationBuilder.DropIndex(
                name: "IX_PrivateReplacements_ReplacementOccurrenceId",
                table: "PrivateReplacements");

            migrationBuilder.DropIndex(
                name: "IX_PrivateOccurrences_PrivateArrangementId",
                table: "PrivateOccurrences");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "PrivateRequests",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "PrivateReplacements",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedByAdminId",
                table: "PrivateReplacements",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateRequests_OccurrenceId_RequestType",
                table: "PrivateRequests",
                columns: new[] { "OccurrenceId", "RequestType" },
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateRequests_OccurrenceId_RequestType_Status",
                table: "PrivateRequests",
                columns: new[] { "OccurrenceId", "RequestType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PrivateRequests_StudentId",
                table: "PrivateRequests",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateReplacements_AssignedByAdminId",
                table: "PrivateReplacements",
                column: "AssignedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateReplacements_ReplacementOccurrenceId",
                table: "PrivateReplacements",
                column: "ReplacementOccurrenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrivateReplacements_StudentId",
                table: "PrivateReplacements",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateOccurrences_PrivateArrangementId_StartUtc",
                table: "PrivateOccurrences",
                columns: new[] { "PrivateArrangementId", "StartUtc" },
                unique: true,
                filter: "[PrivateArrangementId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateReplacements_AspNetUsers_AssignedByAdminId",
                table: "PrivateReplacements",
                column: "AssignedByAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateReplacements_AspNetUsers_StudentId",
                table: "PrivateReplacements",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PrivateRequests_AspNetUsers_StudentId",
                table: "PrivateRequests",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrivateReplacements_AspNetUsers_AssignedByAdminId",
                table: "PrivateReplacements");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivateReplacements_AspNetUsers_StudentId",
                table: "PrivateReplacements");

            migrationBuilder.DropForeignKey(
                name: "FK_PrivateRequests_AspNetUsers_StudentId",
                table: "PrivateRequests");

            migrationBuilder.DropIndex(
                name: "IX_PrivateRequests_OccurrenceId_RequestType",
                table: "PrivateRequests");

            migrationBuilder.DropIndex(
                name: "IX_PrivateRequests_OccurrenceId_RequestType_Status",
                table: "PrivateRequests");

            migrationBuilder.DropIndex(
                name: "IX_PrivateRequests_StudentId",
                table: "PrivateRequests");

            migrationBuilder.DropIndex(
                name: "IX_PrivateReplacements_AssignedByAdminId",
                table: "PrivateReplacements");

            migrationBuilder.DropIndex(
                name: "IX_PrivateReplacements_ReplacementOccurrenceId",
                table: "PrivateReplacements");

            migrationBuilder.DropIndex(
                name: "IX_PrivateReplacements_StudentId",
                table: "PrivateReplacements");

            migrationBuilder.DropIndex(
                name: "IX_PrivateOccurrences_PrivateArrangementId_StartUtc",
                table: "PrivateOccurrences");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "PrivateRequests",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "PrivateReplacements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedByAdminId",
                table: "PrivateReplacements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateRequests_OccurrenceId",
                table: "PrivateRequests",
                column: "OccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateReplacements_ReplacementOccurrenceId",
                table: "PrivateReplacements",
                column: "ReplacementOccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateOccurrences_PrivateArrangementId",
                table: "PrivateOccurrences",
                column: "PrivateArrangementId");
        }
    }
}
