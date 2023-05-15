using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbershop.Migrations
{
    /// <inheritdoc />
    public partial class addedCommentsToAppointments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Appointments");
        }
    }
}
