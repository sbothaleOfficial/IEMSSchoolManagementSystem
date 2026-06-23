using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IEMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchoolEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Key",
                keyValue: "School.Email",
                columns: new[] { "DefaultValue", "Value" },
                values: new object[] { "inspiremardi@gmail.com", "inspiremardi@gmail.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "Key",
                keyValue: "School.Email",
                columns: new[] { "DefaultValue", "Value" },
                values: new object[] { "inspire.mardi@gmail.com", "inspire.mardi@gmail.com" });
        }
    }
}
