-- Add the upload fields required by the document creation API to dbo.Documents.
-- Safe to run more than once; existing columns and records are left unchanged.

IF OBJECT_ID(N'dbo.Documents', N'U') IS NULL
BEGIN
    ;THROW 50001, 'dbo.Documents must exist before adding document upload fields.', 1;
END;
GO

IF COL_LENGTH(N'dbo.Documents', N'FileName') IS NULL
    ALTER TABLE [dbo].[Documents]
        ADD [FileName] nvarchar(max) NOT NULL
            CONSTRAINT [DF_Documents_FileName] DEFAULT N'';
GO

IF COL_LENGTH(N'dbo.Documents', N'ContentType') IS NULL
    ALTER TABLE [dbo].[Documents]
        ADD [ContentType] nvarchar(max) NOT NULL
            CONSTRAINT [DF_Documents_ContentType] DEFAULT N'application/octet-stream';
GO

IF COL_LENGTH(N'dbo.Documents', N'FileData') IS NULL
    ALTER TABLE [dbo].[Documents]
        ADD [FileData] varbinary(max) NOT NULL
            CONSTRAINT [DF_Documents_FileData] DEFAULT 0x;
GO

IF COL_LENGTH(N'dbo.Documents', N'Comment') IS NULL
    ALTER TABLE [dbo].[Documents]
        ADD [Comment] nvarchar(max) NULL;
GO

IF COL_LENGTH(N'dbo.Documents', N'ApprovedBy') IS NULL
    ALTER TABLE [dbo].[Documents]
        ADD [ApprovedBy] int NULL;
GO

IF COL_LENGTH(N'dbo.Documents', N'ApprovedDate') IS NULL
    ALTER TABLE [dbo].[Documents]
        ADD [ApprovedDate] datetime2 NULL;
GO
