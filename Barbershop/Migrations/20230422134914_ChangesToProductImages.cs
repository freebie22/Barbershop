using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbershop.Migrations
{
    /// <inheritdoc />
    public partial class ChangesToProductImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImagesProducts_ProductImages",
                table: "ProductImagesProducts");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ProductImages");

            migrationBuilder.RenameColumn(
                name: "ImagesId",
                table: "ProductImagesProducts",
                newName: "ProductImagesId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImagesProducts_ProductImages_ProductImagesId",
                table: "ProductImagesProducts",
                column: "ProductImagesId",
                principalTable: "ProductImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImagesProducts_ProductImages_ProductImagesId",
                table: "ProductImagesProducts");

            migrationBuilder.RenameColumn(
                name: "ProductImagesId",
                table: "ProductImagesProducts",
                newName: "ProductId");

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ProductImages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImagesProducts_ProductImages_ProductId",
                table: "ProductImagesProducts",
                column: "ProductId",
                principalTable: "ProductImages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
