using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Engineering_Hub.Migrations
{
    /// <inheritdoc />
    public partial class AddPricesToBookableEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Workshops",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Tracks",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "InteractiveActivities",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "InteractiveActivities");
        }
    }
}
