using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CinemaInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllRemoveSeedDataFromTheContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DeleteData(
            //    table: "HallTypes",
            //    keyColumn: "Id",
            //    keyValue: 1);

            //migrationBuilder.DeleteData(
            //    table: "HallTypes",
            //    keyColumn: "Id",
            //    keyValue: 2);

            //migrationBuilder.DeleteData(
            //    table: "HallTypes",
            //    keyColumn: "Id",
            //    keyValue: 3);

            //migrationBuilder.DeleteData(
            //    table: "SeatTypes",
            //    keyColumn: "Id",
            //    keyValue: 1);

            //migrationBuilder.DeleteData(
            //    table: "SeatTypes",
            //    keyColumn: "Id",
            //    keyValue: 2);

            //migrationBuilder.DeleteData(
            //    table: "SeatTypes",
            //    keyColumn: "Id",
            //    keyValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "HallTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Regular cinema hall", "Standard" },
                    { 2, "IMAX large format hall", "IMAX" },
                    { 3, "VIP hall with premium seats", "VIP" }
                });

            migrationBuilder.InsertData(
                table: "SeatTypes",
                columns: new[] { "Id", "Description", "MarkUpInPercentage", "Name" },
                values: new object[,]
                {
                    { 1, "Regular seat", 0m, "Standard" },
                    { 2, "More comfortable seat", 15m, "Comfort" },
                    { 3, "Premium seat", 30m, "VIP" }
                });
        }
    }
}
