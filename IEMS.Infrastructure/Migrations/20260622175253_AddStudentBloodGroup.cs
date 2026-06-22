using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IEMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentBloodGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // New nullable column for the student's blood group (shown on ID cards).
            // Existing seeded rows default to NULL, so no data updates are required.
            migrationBuilder.AddColumn<string>(
                name: "BloodGroup",
                table: "Students",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BloodGroup",
                table: "Students");
        }
    }
}
