using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbershop.Migrations
{
    /// <inheritdoc />
    public partial class changedSomeLogicForAppointmentAndBarbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BarberSchedule_BarbershopUserId",
                table: "BarberSchedule");

            migrationBuilder.DropColumn(
                name: "BarbershopUserId",
                table: "BarberSchedule");

            migrationBuilder.AddColumn<int>(
                name: "BarberId",
                table: "BarberSchedule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "BarberId",
                table: "Appointments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_BarberSchedule_BarberId",
                table: "BarberSchedule",
                column: "BarberId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Barbers_BarberId",
                table: "Appointments",
                column: "BarberId",
                principalTable: "Barbers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BarberSchedule_Barbers_BarberId",
                table: "BarberSchedule",
                column: "BarberId",
                principalTable: "Barbers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Barbers_BarberId",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_BarberSchedule_Barbers_BarberId",
                table: "BarberSchedule");

            migrationBuilder.DropIndex(
                name: "IX_BarberSchedule_BarberId",
                table: "BarberSchedule");

            migrationBuilder.DropColumn(
                name: "BarberId",
                table: "BarberSchedule");

            migrationBuilder.AddColumn<string>(
                name: "BarbershopUserId",
                table: "BarberSchedule",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "BarberId",
                table: "Appointments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_BarberSchedule_BarbershopUserId",
                table: "BarberSchedule",
                column: "BarbershopUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_AspNetUsers_BarberId",
                table: "Appointments",
                column: "BarberId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BarberSchedule_AspNetUsers_BarbershopUserId",
                table: "BarberSchedule",
                column: "BarbershopUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
