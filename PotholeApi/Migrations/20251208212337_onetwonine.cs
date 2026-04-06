using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotholeApi.Migrations
{
    /// <inheritdoc />
    public partial class onetwonine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "Potholes",
                type: "int",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Potholes_Districts_DistrictId",
                table: "Potholes");

            migrationBuilder.DropIndex(
                name: "IX_Potholes_DistrictId",
                table: "Potholes");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Potholes");
        }
    }
}
