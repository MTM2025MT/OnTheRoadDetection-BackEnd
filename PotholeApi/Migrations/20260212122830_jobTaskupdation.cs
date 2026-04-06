using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotholeApi.Migrations
{
    /// <inheritdoc />
    public partial class jobTaskupdation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Budget",
                table: "JobTasks",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Materials",
                table: "JobTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "JobTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "JobTasks",
                type: "int",
                nullable: true,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Budget",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "Materials",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "JobTasks");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "JobTasks");
        }
    }
}
