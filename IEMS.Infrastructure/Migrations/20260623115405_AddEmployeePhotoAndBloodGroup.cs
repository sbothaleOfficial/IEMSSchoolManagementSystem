using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IEMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeePhotoAndBloodGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BloodGroup",
                table: "Teachers",
                type: "TEXT",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Photo",
                table: "Teachers",
                type: "BLOB",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BloodGroup",
                table: "Staff",
                type: "TEXT",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Photo",
                table: "Staff",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloodGroup",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "BloodGroup",
                table: "Staff");

            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Staff");
        }
    }
}
