USE [TRG_CORE];
GO

IF OBJECT_ID(N'dbo.KS_Roles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KS_Roles
    (
        RoleId int IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_KS_Roles PRIMARY KEY,
        Name nvarchar(50) NOT NULL,
        CONSTRAINT UQ_KS_Roles_Name UNIQUE (Name)
    );
END;
GO

IF OBJECT_ID(N'dbo.KS_RecallUsers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KS_RecallUsers
    (
        UserId int IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_KS_RecallUsers PRIMARY KEY,
        Username nvarchar(450) NOT NULL,
        Password nvarchar(max) NOT NULL,
        Role nvarchar(max) NOT NULL
            CONSTRAINT DF_KS_RecallUsers_Role DEFAULT (N''),
        FullName nvarchar(max) NOT NULL
            CONSTRAINT DF_KS_RecallUsers_FullName DEFAULT (N''),
        Email nvarchar(max) NOT NULL
            CONSTRAINT DF_KS_RecallUsers_Email DEFAULT (N''),
        RegistrationStatus nvarchar(max) NOT NULL
            CONSTRAINT DF_KS_RecallUsers_RegistrationStatus DEFAULT (N'Pending'),
        IsActive bit NOT NULL
            CONSTRAINT DF_KS_RecallUsers_IsActive DEFAULT (1),
        CreatedAt datetime2 NOT NULL
            CONSTRAINT DF_KS_RecallUsers_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT UQ_KS_RecallUsers_Username UNIQUE (Username)
    );
END;
GO

IF OBJECT_ID(N'dbo.KS_UserRoles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.KS_UserRoles
    (
        UserId int NOT NULL,
        RoleId int NOT NULL,
        CONSTRAINT PK_KS_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_KS_UserRoles_KS_RecallUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.KS_RecallUsers (UserId)
            ON DELETE CASCADE,
        CONSTRAINT FK_KS_UserRoles_KS_Roles_RoleId
            FOREIGN KEY (RoleId) REFERENCES dbo.KS_Roles (RoleId)
            ON DELETE CASCADE
    );
END;
GO

INSERT INTO dbo.KS_Roles (Name)
SELECT role.Name
FROM (VALUES (N'Admin'), (N'Operator'), (N'Supervisor')) AS role(Name)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.KS_Roles AS existingRole
    WHERE existingRole.Name = role.Name
);
GO
