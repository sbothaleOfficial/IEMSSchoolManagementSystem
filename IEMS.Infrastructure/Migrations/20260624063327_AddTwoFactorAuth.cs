using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IEMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoFactorAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TwoFactorBackupCodes",
                table: "Users",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorSecret",
                table: "Users",
                type: "TEXT",
                maxLength: 64,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "TwoFactorBackupCodes", "TwoFactorEnabled", "TwoFactorSecret" },
                values: new object[] { null, false, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwoFactorBackupCodes",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorSecret",
                table: "Users");
        }
    }
}
