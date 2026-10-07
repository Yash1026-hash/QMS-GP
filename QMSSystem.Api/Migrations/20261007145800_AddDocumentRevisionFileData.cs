using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QMSSystem.Api.Data;

#nullable disable

namespace QMSSystem.Api.Migrations;

[DbContext(typeof(QmsDbContext))]
[Migration("20261007145800_AddDocumentRevisionFileData")]
public sealed class AddDocumentRevisionFileData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF COL_LENGTH(N'dbo.KS_DocumentRevisions', N'ContentType') IS NULL
                ALTER TABLE [dbo].[KS_DocumentRevisions]
                ADD [ContentType] nvarchar(255) NOT NULL
                    CONSTRAINT [DF_KS_DocumentRevisions_ContentType] DEFAULT N'application/octet-stream';

            IF COL_LENGTH(N'dbo.KS_DocumentRevisions', N'FileData') IS NULL
                ALTER TABLE [dbo].[KS_DocumentRevisions]
                ADD [FileData] varbinary(max) NOT NULL
                    CONSTRAINT [DF_KS_DocumentRevisions_FileData] DEFAULT 0x;

            IF COL_LENGTH(N'dbo.KS_DocumentRevisions', N'Comment') IS NULL
                ALTER TABLE [dbo].[KS_DocumentRevisions]
                ADD [Comment] nvarchar(max) NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            IF COL_LENGTH(N'dbo.KS_DocumentRevisions', N'ContentType') IS NOT NULL
                ALTER TABLE [dbo].[KS_DocumentRevisions] DROP COLUMN [ContentType];

            IF COL_LENGTH(N'dbo.KS_DocumentRevisions', N'FileData') IS NOT NULL
                ALTER TABLE [dbo].[KS_DocumentRevisions] DROP COLUMN [FileData];

            IF COL_LENGTH(N'dbo.KS_DocumentRevisions', N'Comment') IS NOT NULL
                ALTER TABLE [dbo].[KS_DocumentRevisions] DROP COLUMN [Comment];
            """);
    }
}
