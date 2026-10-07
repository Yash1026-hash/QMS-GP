using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QMSSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class CreateDocumentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "Documents",
                newSchema: "dbo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Documents",
                schema: "dbo",
                newName: "Documents");
        }
    }
}
