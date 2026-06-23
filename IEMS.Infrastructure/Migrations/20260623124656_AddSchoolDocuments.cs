using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IEMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchoolDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DocumentType = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UploadedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolDocuments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolDocuments_UploadedAt",
                table: "SchoolDocuments",
                column: "UploadedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolDocuments");
        }
    }
}
