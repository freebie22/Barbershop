using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbershop.Migrations
{
    /// <inheritdoc />
    public partial class addedRequiredSpecializationToServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpecializationId",
                table: "Services",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_SpecializationId",
                table: "Services",
                column: "SpecializationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Specializations_SpecializationId",
                table: "Services",
                column: "SpecializationId",
                principalTable: "Specializations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Specializations_SpecializationId",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_SpecializationId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "SpecializationId",
                table: "Services");
        }
    }
}
