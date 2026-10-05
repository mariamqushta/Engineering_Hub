using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engineering_Hub.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTrackPackageSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackPackageBookings");

            migrationBuilder.DropTable(
                name: "TrackPackageInteractives");

            migrationBuilder.DropTable(
                name: "TrackPackageWorkshops");

            migrationBuilder.DropTable(
                name: "TrackPackages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IncludesWorkshop = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackPackages_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackPackageBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackPackageId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BookedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackPackageBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackPackageBookings_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackPackageBookings_TrackPackages_TrackPackageId",
                        column: x => x.TrackPackageId,
                        principalTable: "TrackPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackPackageInteractives",
                columns: table => new
                {
                    TrackPackageId = table.Column<int>(type: "int", nullable: false),
                    InteractiveActivityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackPackageInteractives", x => new { x.TrackPackageId, x.InteractiveActivityId });
                    table.ForeignKey(
                        name: "FK_TrackPackageInteractives_InteractiveActivities_InteractiveActivityId",
                        column: x => x.InteractiveActivityId,
                        principalTable: "InteractiveActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackPackageInteractives_TrackPackages_TrackPackageId",
                        column: x => x.TrackPackageId,
                        principalTable: "TrackPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrackPackageWorkshops",
                columns: table => new
                {
                    TrackPackageId = table.Column<int>(type: "int", nullable: false),
                    WorkshopId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackPackageWorkshops", x => new { x.TrackPackageId, x.WorkshopId });
                    table.ForeignKey(
                        name: "FK_TrackPackageWorkshops_TrackPackages_TrackPackageId",
                        column: x => x.TrackPackageId,
                        principalTable: "TrackPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrackPackageWorkshops_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageBookings_TrackPackageId",
                table: "TrackPackageBookings",
                column: "TrackPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageBookings_UserId",
                table: "TrackPackageBookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageInteractives_InteractiveActivityId",
                table: "TrackPackageInteractives",
                column: "InteractiveActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackages_TrackId",
                table: "TrackPackages",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageWorkshops_WorkshopId",
                table: "TrackPackageWorkshops",
                column: "WorkshopId");
        }
    }
}
