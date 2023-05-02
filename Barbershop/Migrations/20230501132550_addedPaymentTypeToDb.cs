using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbershop.Migrations
{
    /// <inheritdoc />
    public partial class addedPaymentTypeToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentType",
                table: "OrderHeader",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentType",
                table: "OrderHeader");
        }
    }
}
