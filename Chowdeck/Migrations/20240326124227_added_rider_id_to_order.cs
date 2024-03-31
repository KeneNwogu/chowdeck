using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chowdeck.Migrations
{
    /// <inheritdoc />
    public partial class added_rider_id_to_order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RiderId",
                table: "Orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RiderId",
                table: "Orders");
        }
    }
}
