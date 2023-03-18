using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbershop.Migrations
{
    /// <inheritdoc />
    public partial class addedBarbersToDbAndRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Barbers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exprerience = table.Column<int>(type: "int", nullable: false),
                    BarberImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkPositionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Barbers_WorkPositions_WorkPositionId",
                        column: x => x.WorkPositionId,
                        principalTable: "WorkPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BarbersSpecializations",
                columns: table => new
                {
                    BarbersId = table.Column<int>(type: "int", nullable: false),
                    SpecializationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BarbersSpecializations", x => new { x.BarbersId, x.SpecializationsId });
                    table.ForeignKey(
                        name: "FK_BarbersSpecializations_Barbers_BarbersId",
                        column: x => x.BarbersId,
                        principalTable: "Barbers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BarbersSpecializations_Specializations_SpecializationsId",
                        column: x => x.SpecializationsId,
                        principalTable: "Specializations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Barbers_WorkPositionId",
                table: "Barbers",
                column: "WorkPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_BarbersSpecializations_SpecializationsId",
                table: "BarbersSpecializations",
                column: "SpecializationsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarbersSpecializations");

            migrationBuilder.DropTable(
                name: "Barbers");
        }
    }
}
