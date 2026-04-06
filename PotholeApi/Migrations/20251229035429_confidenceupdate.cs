using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotholeApi.Migrations
{
    /// <inheritdoc />
    public partial class confidenceupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "confidence",
                table: "Potholes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "confidence",
                table: "Potholes");
        }
    }
}
