using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbershop.Migrations
{
    /// <inheritdoc />
    public partial class addedSomeNewLogicToBarbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BarbershopUserId",
                table: "Barbers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                table: "Barbers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Barbers_BarbershopUserId",
                table: "Barbers",
                column: "BarbershopUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Barbers_AspNetUsers_BarbershopUserId",
                table: "Barbers",
                column: "BarbershopUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Barbers_AspNetUsers_BarbershopUserId",
                table: "Barbers");

            migrationBuilder.DropIndex(
                name: "IX_Barbers_BarbershopUserId",
                table: "Barbers");

            migrationBuilder.DropColumn(
                name: "BarbershopUserId",
                table: "Barbers");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Barbers");
        }
    }
}
