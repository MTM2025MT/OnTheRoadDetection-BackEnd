using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotholeApi.Migrations
{
    /// <inheritdoc />
    public partial class onetwoonetwosecond : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Streets_neighborhoods_NeighborhoodId",
                table: "Streets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_neighborhoods",
                table: "neighborhoods");

            migrationBuilder.RenameTable(
                name: "neighborhoods",
                newName: "Neighborhood");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Neighborhood",
                table: "Neighborhood",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Streets_Neighborhood_NeighborhoodId",
                table: "Streets",
                column: "NeighborhoodId",
                principalTable: "Neighborhood",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Streets_Neighborhood_NeighborhoodId",
                table: "Streets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Neighborhood",
                table: "Neighborhood");

            migrationBuilder.RenameTable(
                name: "Neighborhood",
                newName: "neighborhoods");

            migrationBuilder.AddPrimaryKey(
                name: "PK_neighborhoods",
                table: "neighborhoods",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Streets_neighborhoods_NeighborhoodId",
                table: "Streets",
                column: "NeighborhoodId",
                principalTable: "neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
