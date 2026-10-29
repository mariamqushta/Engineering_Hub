using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engineering_Hub.Migrations
{
    /// <inheritdoc />
    public partial class AddInstructorOwnershipToSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Workshops",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructorId",
                table: "Workshops",
                type: "nvarchar(450)",
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "InteractiveActivities",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructorId",
                table: "InteractiveActivities",
                type: "nvarchar(450)",
                nullable: true,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_ApplicationUserId",
                table: "Workshops",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_InstructorId",
                table: "Workshops",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveActivities_ApplicationUserId",
                table: "InteractiveActivities",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractiveActivities_InstructorId",
                table: "InteractiveActivities",
                column: "InstructorId");

            migrationBuilder.AddForeignKey(
                name: "FK_InteractiveActivities_AspNetUsers_ApplicationUserId",
                table: "InteractiveActivities",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InteractiveActivities_AspNetUsers_InstructorId",
                table: "InteractiveActivities",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_AspNetUsers_ApplicationUserId",
                table: "Workshops",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_AspNetUsers_InstructorId",
                table: "Workshops",
                column: "InstructorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InteractiveActivities_AspNetUsers_ApplicationUserId",
                table: "InteractiveActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_InteractiveActivities_AspNetUsers_InstructorId",
                table: "InteractiveActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_AspNetUsers_ApplicationUserId",
                table: "Workshops");

            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_AspNetUsers_InstructorId",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_ApplicationUserId",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_InstructorId",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_InteractiveActivities_ApplicationUserId",
                table: "InteractiveActivities");

            migrationBuilder.DropIndex(
                name: "IX_InteractiveActivities_InstructorId",
                table: "InteractiveActivities");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "InstructorId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "InteractiveActivities");

            migrationBuilder.DropColumn(
                name: "InstructorId",
                table: "InteractiveActivities");
        }
    }
}
