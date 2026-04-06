using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotholeApi.Migrations
{
    /// <inheritdoc />
    public partial class onetwoonetwothird : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Neighbourhoods_Districts_DistrictId",
                table: "Neighbourhoods");

            migrationBuilder.DropForeignKey(
                name: "FK_Streets_Neighborhood_NeighborhoodId",
                table: "Streets");

            migrationBuilder.DropTable(
                name: "Neighborhood");

            migrationBuilder.RenameColumn(
                name: "NeighborhoodId",
                table: "Streets",
                newName: "NeighbourhoodId");

            migrationBuilder.RenameIndex(
                name: "IX_Streets_NeighborhoodId",
                table: "Streets",
                newName: "IX_Streets_NeighbourhoodId");

            migrationBuilder.AlterColumn<int>(
                name: "DistrictId",
                table: "Neighbourhoods",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Neighbourhoods_Districts_DistrictId",
                table: "Neighbourhoods",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Streets_Neighbourhoods_NeighbourhoodId",
                table: "Streets",
                column: "NeighbourhoodId",
                principalTable: "Neighbourhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Neighbourhoods_Districts_DistrictId",
                table: "Neighbourhoods");

            migrationBuilder.DropForeignKey(
                name: "FK_Streets_Neighbourhoods_NeighbourhoodId",
                table: "Streets");

            migrationBuilder.RenameColumn(
                name: "NeighbourhoodId",
                table: "Streets",
                newName: "NeighborhoodId");

            migrationBuilder.RenameIndex(
                name: "IX_Streets_NeighbourhoodId",
                table: "Streets",
                newName: "IX_Streets_NeighborhoodId");

            migrationBuilder.AlterColumn<int>(
                name: "DistrictId",
                table: "Neighbourhoods",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Neighborhood",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Neighborhood", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Neighbourhoods_Districts_DistrictId",
                table: "Neighbourhoods",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Streets_Neighborhood_NeighborhoodId",
                table: "Streets",
                column: "NeighborhoodId",
                principalTable: "Neighborhood",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
