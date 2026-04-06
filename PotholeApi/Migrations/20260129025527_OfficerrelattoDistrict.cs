using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotholeApi.Migrations
{
    /// <inheritdoc />
    public partial class OfficerrelattoDistrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "Officers",
                type: "int",
                nullable: false,
                defaultValue: 1005);

            migrationBuilder.CreateIndex(
                name: "IX_Officers_DistrictId",
                table: "Officers",
                column: "DistrictId");

            migrationBuilder.AddForeignKey(
                name: "FK_Officers_Districts_DistrictId",
                table: "Officers",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Officers_Districts_DistrictId",
                table: "Officers");

            migrationBuilder.DropIndex(
                name: "IX_Officers_DistrictId",
                table: "Officers");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Officers");
        }
    }
}
