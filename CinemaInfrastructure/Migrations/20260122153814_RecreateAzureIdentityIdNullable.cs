using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecreateAzureIdentityIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_AzureIdentityId",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "AzureIdentityId",
                table: "Users",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AzureIdentityId",
                table: "Users",
                column: "AzureIdentityId",
                unique: true,
                filter: "[AzureIdentityId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_AzureIdentityId",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "AzureIdentityId",
                table: "Users",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AzureIdentityId",
                table: "Users",
                column: "AzureIdentityId",
                unique: true);
        }
    }
}
