using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbershop.Migrations
{
    /// <inheritdoc />
    public partial class addedAppointmentsAndOrdersCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AppointmentCount",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppointmentPoints",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AppointmentSuccessCount",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderCount",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderPoints",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderSuccessCount",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppointmentCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AppointmentPoints",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AppointmentSuccessCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrderCount",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrderPoints",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "OrderSuccessCount",
                table: "AspNetUsers");
        }
    }
}
