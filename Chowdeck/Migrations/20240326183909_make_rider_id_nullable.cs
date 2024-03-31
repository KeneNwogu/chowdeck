using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chowdeck.Migrations
{
    /// <inheritdoc />
    public partial class make_rider_id_nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderTimelines_Users_RiderId",
                table: "OrderTimelines");

            migrationBuilder.AlterColumn<string>(
                name: "RiderId",
                table: "OrderTimelines",
                type: "character varying(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderTimelines_Users_RiderId",
                table: "OrderTimelines",
                column: "RiderId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderTimelines_Users_RiderId",
                table: "OrderTimelines");

            migrationBuilder.AlterColumn<string>(
                name: "RiderId",
                table: "OrderTimelines",
                type: "character varying(255)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderTimelines_Users_RiderId",
                table: "OrderTimelines",
                column: "RiderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
