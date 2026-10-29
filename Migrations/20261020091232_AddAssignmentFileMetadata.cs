using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engineering_Hub.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentFileMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "AssignmentSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "AssignmentSubmissions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "AssignmentSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoredFileName",
                table: "AssignmentSubmissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentContentType",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AttachmentFileSize",
                table: "Assignments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentOriginalFileName",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentStoredFileName",
                table: "Assignments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "StoredFileName",
                table: "AssignmentSubmissions");

            migrationBuilder.DropColumn(
                name: "AttachmentContentType",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "AttachmentFileSize",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "AttachmentOriginalFileName",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "AttachmentStoredFileName",
                table: "Assignments");
        }
    }
}
