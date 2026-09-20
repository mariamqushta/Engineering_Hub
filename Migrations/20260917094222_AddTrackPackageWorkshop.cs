using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engineering_Hub.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackPackageWorkshop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_TrackPackageInteractives_InteractiveActivityId",
                table: "TrackPackageInteractives",
                column: "InteractiveActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackPackageWorkshops_WorkshopId",
                table: "TrackPackageWorkshops",
                column: "WorkshopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackPackageInteractives");

            migrationBuilder.DropTable(
                name: "TrackPackageWorkshops");
        }
    }
}
