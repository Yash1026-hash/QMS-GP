using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QMSSystem.Api.Migrations;

public partial class AddDocumentEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "DocumentEvents",
            schema: "dbo",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                DocumentId = table.Column<int>(type: "int", nullable: false),
                EventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                ActorUserId = table.Column<int>(type: "int", nullable: false),
                EventOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DocumentEvents", eventRow => eventRow.Id);
                table.ForeignKey(
                    name: "FK_DocumentEvents_DocumentCreations_DocumentId",
                    column: eventRow => eventRow.DocumentId,
                    principalSchema: "dbo",
                    principalTable: "DocumentCreations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_DocumentEvents_DocumentId",
            schema: "dbo",
            table: "DocumentEvents",
            column: "DocumentId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "DocumentEvents",
            schema: "dbo");
    }
}
