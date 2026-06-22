using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IEMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // New nullable BLOB column for an optional student photo (used on ID cards).
            // Existing seeded rows default to NULL, so no data updates are required.
            migrationBuilder.AddColumn<byte[]>(
                name: "Photo",
                table: "Students",
                type: "BLOB",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Students");
        }
    }
}
