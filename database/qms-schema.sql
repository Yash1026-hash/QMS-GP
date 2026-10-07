-- QMS module tables (documents, deviations, change requests, approvals, audit log).
-- Generated from QMSSystem.Api/Data/QmsDbContext.cs. If you change the models, regenerate it.
-- Safe to run more than once: each table is created only when it does not exist yet.
-- It never changes or drops an existing table. Run it after database/login-schema.sql.

USE [TRG_CORE];
GO

IF OBJECT_ID(N'dbo.Documents', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Documents] (
        [Id] int NOT NULL IDENTITY,
        [DocumentNumber] nvarchar(50) NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Department] nvarchar(100) NOT NULL,
        [CurrentVersion] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedBy] int NOT NULL,
        [CreationTime] datetime2 NOT NULL,
        [FileName] nvarchar(max) NOT NULL,
        [ContentType] nvarchar(max) NOT NULL,
        [FileData] varbinary(max) NOT NULL,
        [Comment] nvarchar(max) NULL,
        [ApprovedBy] int NULL,
        [ApprovedDate] datetime2 NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_Documents_DocumentNumber] ON [dbo].[Documents] ([DocumentNumber]);
END;
GO

IF OBJECT_ID(N'dbo.KS_Deviations', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KS_Deviations] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [DocumentId] int NOT NULL,
        [Priority] nvarchar(20) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [ClosedBy] nvarchar(max) NULL,
        [ClosedDate] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_KS_Deviations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KS_Deviations_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_KS_Deviations_DocumentId] ON [dbo].[KS_Deviations] ([DocumentId]);
END;
GO

IF OBJECT_ID(N'dbo.KS_DeviationAttachments', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KS_DeviationAttachments] (
        [Id] int NOT NULL IDENTITY,
        [DeviationId] int NOT NULL,
        [FileName] nvarchar(260) NOT NULL,
        [StoredPath] nvarchar(max) NOT NULL,
        [ContentType] nvarchar(max) NOT NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_KS_DeviationAttachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KS_DeviationAttachments_KS_Deviations_DeviationId] FOREIGN KEY ([DeviationId]) REFERENCES [dbo].[KS_Deviations] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_KS_DeviationAttachments_DeviationId] ON [dbo].[KS_DeviationAttachments] ([DeviationId]);
END;
GO

IF OBJECT_ID(N'dbo.KS_DeviationReports', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KS_DeviationReports] (
        [Id] int NOT NULL IDENTITY,
        [DeviationId] int NOT NULL,
        [AttemptNumber] int NOT NULL,
        [Summary] nvarchar(max) NOT NULL,
        [FileName] nvarchar(260) NOT NULL,
        [StoredPath] nvarchar(max) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [RootCause] nvarchar(max) NOT NULL,
        [CorrectiveAction] nvarchar(max) NOT NULL,
        [ChangeRequired] bit NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_KS_DeviationReports] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KS_DeviationReports_KS_Deviations_DeviationId] FOREIGN KEY ([DeviationId]) REFERENCES [dbo].[KS_Deviations] ([Id]) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX [IX_KS_DeviationReports_DeviationId_AttemptNumber] ON [dbo].[KS_DeviationReports] ([DeviationId], [AttemptNumber]);
END;
GO

IF OBJECT_ID(N'dbo.KS_ChangeRequests', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KS_ChangeRequests] (
        [Id] int NOT NULL IDENTITY,
        [DocumentId] int NOT NULL,
        [DeviationId] int NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [ChangeType] nvarchar(20) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [RequestedByUserId] int NOT NULL,
        [RequestedDate] datetime2 NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [DeviationReportId] int NULL,
        CONSTRAINT [PK_KS_ChangeRequests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KS_ChangeRequests_KS_DeviationReports_DeviationReportId] FOREIGN KEY ([DeviationReportId]) REFERENCES [dbo].[KS_DeviationReports] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_KS_ChangeRequests_KS_Deviations_DeviationId] FOREIGN KEY ([DeviationId]) REFERENCES [dbo].[KS_Deviations] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_KS_ChangeRequests_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_KS_ChangeRequests_DeviationId] ON [dbo].[KS_ChangeRequests] ([DeviationId]);

    CREATE INDEX [IX_KS_ChangeRequests_DeviationReportId] ON [dbo].[KS_ChangeRequests] ([DeviationReportId]);

    CREATE INDEX [IX_KS_ChangeRequests_DocumentId] ON [dbo].[KS_ChangeRequests] ([DocumentId]);
END;
GO

IF OBJECT_ID(N'dbo.KS_DocumentRevisions', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KS_DocumentRevisions] (
        [Id] int NOT NULL IDENTITY,
        [DocumentId] int NOT NULL,
        [Version] int NOT NULL,
        [FileName] nvarchar(260) NOT NULL,
        [ContentType] nvarchar(255) NOT NULL,
        [FileData] varbinary(max) NOT NULL,
        [Comment] nvarchar(max) NULL,
        [UploadedBy] nvarchar(max) NOT NULL,
        [UploadedTime] datetime2 NOT NULL,
        [ChangeSummary] nvarchar(max) NOT NULL,
        [ApprovalStatus] nvarchar(20) NOT NULL,
        [ApprovedBy] nvarchar(max) NULL,
        [ApprovalDate] datetime2 NULL,
        [ChangeRequestId] int NULL,
        CONSTRAINT [PK_KS_DocumentRevisions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KS_DocumentRevisions_KS_ChangeRequests_ChangeRequestId] FOREIGN KEY ([ChangeRequestId]) REFERENCES [dbo].[KS_ChangeRequests] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_KS_DocumentRevisions_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_KS_DocumentRevisions_ChangeRequestId] ON [dbo].[KS_DocumentRevisions] ([ChangeRequestId]);

    CREATE INDEX [IX_KS_DocumentRevisions_DocumentId] ON [dbo].[KS_DocumentRevisions] ([DocumentId]);
END;
GO

IF OBJECT_ID(N'dbo.KS_ApprovalRecords', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KS_ApprovalRecords] (
        [Id] int NOT NULL IDENTITY,
        [ItemType] nvarchar(30) NOT NULL,
        [ItemId] int NOT NULL,
        [Decision] nvarchar(20) NOT NULL,
        [Comments] nvarchar(max) NOT NULL,
        [ReviewedByUserId] int NOT NULL,
        [DecisionDate] datetime2 NOT NULL,
        CONSTRAINT [PK_KS_ApprovalRecords] PRIMARY KEY ([Id])
    );

    CREATE UNIQUE INDEX [IX_KS_ApprovalRecords_ItemType_ItemId] ON [dbo].[KS_ApprovalRecords] ([ItemType], [ItemId]);
END;
GO

IF OBJECT_ID(N'dbo.KS_AuditLogs', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KS_AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [ItemType] nvarchar(30) NOT NULL,
        [ItemId] int NOT NULL,
        [DeviationId] int NULL,
        [Action] nvarchar(100) NOT NULL,
        [ActorUserId] int NULL,
        [Timestamp] datetime2 NOT NULL,
        [Comment] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_KS_AuditLogs] PRIMARY KEY ([Id])
    );

    CREATE INDEX [IX_KS_AuditLogs_DeviationId] ON [dbo].[KS_AuditLogs] ([DeviationId]);

    CREATE INDEX [IX_KS_AuditLogs_ItemType_ItemId] ON [dbo].[KS_AuditLogs] ([ItemType], [ItemId]);
END;
GO

IF OBJECT_ID(N'dbo.KS_ReviewComments', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KS_ReviewComments] (
        [Id] int NOT NULL IDENTITY,
        [DeviationReportId] int NOT NULL,
        [Comment] nvarchar(max) NOT NULL,
        [CommentedBy] nvarchar(50) NOT NULL,
        [CommentDate] datetime2 NOT NULL,
        CONSTRAINT [PK_KS_ReviewComments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KS_ReviewComments_KS_DeviationReports_DeviationReportId] FOREIGN KEY ([DeviationReportId]) REFERENCES [dbo].[KS_DeviationReports] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_KS_ReviewComments_DeviationReportId] ON [dbo].[KS_ReviewComments] ([DeviationReportId]);
END;
GO

IF OBJECT_ID(N'dbo.KS_ReviewProofs', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[KS_ReviewProofs] (
        [Id] int NOT NULL IDENTITY,
        [DeviationReportId] int NOT NULL,
        [FindingId] int NULL,
        [ProofName] nvarchar(260) NOT NULL,
        [ProofType] nvarchar(50) NOT NULL,
        [FilePath] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [UploadedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_KS_ReviewProofs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_KS_ReviewProofs_KS_DeviationReports_DeviationReportId] FOREIGN KEY ([DeviationReportId]) REFERENCES [dbo].[KS_DeviationReports] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_KS_ReviewProofs_DeviationReportId] ON [dbo].[KS_ReviewProofs] ([DeviationReportId]);
END;
GO
