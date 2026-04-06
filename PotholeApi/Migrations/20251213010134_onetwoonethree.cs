using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotholeApi.Migrations
{
    /// <inheritdoc />
    public partial class onetwoonethree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Potholes_Districts_DistrictId",
                table: "Potholes");

            migrationBuilder.DropIndex(
                name: "IX_Potholes_DistrictId",
                table: "Potholes");

            migrationBuilder.AddColumn<int>(
                name: "NeighbourhoodId",
                table: "Potholes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Potholes_NeighbourhoodId",
                table: "Potholes",
                column: "NeighbourhoodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Potholes_Neighbourhoods_NeighbourhoodId",
                table: "Potholes",
                column: "NeighbourhoodId",
                principalTable: "Neighbourhoods",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Potholes_Neighbourhoods_NeighbourhoodId",
                table: "Potholes");

            migrationBuilder.DropIndex(
                name: "IX_Potholes_NeighbourhoodId",
                table: "Potholes");

            migrationBuilder.DropColumn(
                name: "NeighbourhoodId",
                table: "Potholes");

            migrationBuilder.CreateIndex(
                name: "IX_Potholes_DistrictId",
                table: "Potholes",
                column: "DistrictId");

            migrationBuilder.AddForeignKey(
                name: "FK_Potholes_Districts_DistrictId",
                table: "Potholes",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id");
        }
    }
}
