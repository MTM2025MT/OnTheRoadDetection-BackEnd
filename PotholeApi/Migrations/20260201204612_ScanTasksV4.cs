using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotholeApi.Migrations
{
    /// <inheritdoc />
    public partial class ScanTasksV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coordinates_ScanExecutionTasks_ScanExecutionTaskId",
                table: "Coordinates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Coordinates",
                table: "Coordinates");

            migrationBuilder.RenameTable(
                name: "Coordinates",
                newName: "GpsCoordinates");

            migrationBuilder.RenameIndex(
                name: "IX_Coordinates_ScanExecutionTaskId",
                table: "GpsCoordinates",
                newName: "IX_GpsCoordinates_ScanExecutionTaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GpsCoordinates",
                table: "GpsCoordinates",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GpsCoordinates_ScanExecutionTasks_ScanExecutionTaskId",
                table: "GpsCoordinates",
                column: "ScanExecutionTaskId",
                principalTable: "ScanExecutionTasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GpsCoordinates_ScanExecutionTasks_ScanExecutionTaskId",
                table: "GpsCoordinates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GpsCoordinates",
                table: "GpsCoordinates");

            migrationBuilder.RenameTable(
                name: "GpsCoordinates",
                newName: "Coordinates");

            migrationBuilder.RenameIndex(
                name: "IX_GpsCoordinates_ScanExecutionTaskId",
                table: "Coordinates",
                newName: "IX_Coordinates_ScanExecutionTaskId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Coordinates",
                table: "Coordinates",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Coordinates_ScanExecutionTasks_ScanExecutionTaskId",
                table: "Coordinates",
                column: "ScanExecutionTaskId",
                principalTable: "ScanExecutionTasks",
                principalColumn: "Id");
        }
    }
}
