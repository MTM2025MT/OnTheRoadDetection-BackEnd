using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PotholeApi.Migrations
{
    /// <inheritdoc />
    public partial class potholeRepoupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Remove the old Index
            migrationBuilder.DropIndex(
                name: "IX_Potholes_Latitude_Longitude",
                table: "Potholes");

            // 2. Remove the old Columns
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Potholes");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Potholes");

            // 3. ADD THIS: Create the Spatial Index (R-Tree) manually
            // This replaces the .IsSpatial() command that didn't work.
            migrationBuilder.Sql(
                @"CREATE SPATIAL INDEX [IX_Potholes_Location] 
                  ON [Potholes] ([Location]) 
                  USING GEOGRAPHY_GRID 
                  WITH (
                    GRIDS = (LEVEL_1 = MEDIUM, LEVEL_2 = MEDIUM, LEVEL_3 = MEDIUM, LEVEL_4 = MEDIUM), 
                    CELLS_PER_OBJECT = 16
                  );"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Remove the Spatial Index if we roll back
            migrationBuilder.Sql("DROP INDEX [IX_Potholes_Location] ON [Potholes]");

            // 2. Add back the old columns
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Potholes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Potholes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            // 3. Add back the old Index
            migrationBuilder.CreateIndex(
                name: "IX_Potholes_Latitude_Longitude",
                table: "Potholes",
                columns: new[] { "Latitude", "Longitude" });
        }
    }
}