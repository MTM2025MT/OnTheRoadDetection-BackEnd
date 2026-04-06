using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace PotholeApi.Migrations
{
    /// <inheritdoc />
    public partial class dayonetwoeightsecond : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "Geometry",
                table: "Districts",
                type: "geography",
                nullable: true,
                oldClrType: typeof(MultiPolygon),
                oldType: "geography");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "Geometry",
                table: "Districts",
                type: "geography",
                nullable: false,
                oldClrType: typeof(MultiPolygon),
                oldType: "geography",
                oldNullable: true);
        }
    }
}
