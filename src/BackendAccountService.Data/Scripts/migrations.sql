IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [EnrolmentStatuses] (
        [Id] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_EnrolmentStatuses] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [InterOrganisationRoles] (
        [Id] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_InterOrganisationRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [InterPersonRoles] (
        [Id] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_InterPersonRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [Nations] (
        [Id] int NOT NULL,
        [Name] nvarchar(54) NOT NULL,
        CONSTRAINT [PK_Nations] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [OrganisationToPersonRoles] (
        [Id] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_OrganisationToPersonRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [OrganisationTypes] (
        [Id] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_OrganisationTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [PersonInOrganisationRoles] (
        [Id] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_PersonInOrganisationRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [Services] (
        [Id] int NOT NULL IDENTITY,
        [Key] nvarchar(100) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(2000) NULL,
        CONSTRAINT [PK_Services] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ExternalIdpId] nvarchar(200) NULL,
        [ExternalIdpUserId] nvarchar(200) NULL,
        [Email] nvarchar(254) NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
    DECLARE @defaultSchema AS sysname;
    SET @defaultSchema = SCHEMA_NAME();
    DECLARE @description AS sql_variant;
    SET @description = N'External Provider Identity ID';
    EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Users', 'COLUMN', N'ExternalIdpId';
    SET @description = N'External Provider Identity User ID';
    EXEC sp_addextendedproperty 'MS_Description', @description, 'SCHEMA', @defaultSchema, 'TABLE', N'Users', 'COLUMN', N'ExternalIdpUserId';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [Organisations] (
        [Id] int NOT NULL IDENTITY,
        [OrganisationTypeId] int NOT NULL,
        [CompaniesHouseNumber] nvarchar(30) NULL,
        [Name] nvarchar(160) NOT NULL,
        [TradingName] nvarchar(170) NULL,
        [ReferenceNumber] nvarchar(10) NULL,
        [SubBuildingName] nvarchar(100) NULL,
        [BuildingName] nvarchar(100) NULL,
        [BuildingNumber] nvarchar(50) NULL,
        [Street] nvarchar(100) NULL,
        [Locality] nvarchar(100) NULL,
        [DependentLocality] nvarchar(100) NULL,
        [Town] nvarchar(70) NOT NULL,
        [County] nvarchar(50) NULL,
        [Country] nvarchar(54) NULL,
        [Postcode] nvarchar(15) NOT NULL,
        [ValidatedWithCompaniesHouse] bit NOT NULL,
        [IsComplianceScheme] bit NOT NULL,
        [NationId] int NULL,
        [ExternalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [CreatedBy] nvarchar(254) NOT NULL,
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [LastUpdatedBy] nvarchar(254) NOT NULL,
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Organisations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Organisations_Nations_NationId] FOREIGN KEY ([NationId]) REFERENCES [Nations] ([Id]),
        CONSTRAINT [FK_Organisations_OrganisationTypes_OrganisationTypeId] FOREIGN KEY ([OrganisationTypeId]) REFERENCES [OrganisationTypes] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [ServiceRoles] (
        [Id] int NOT NULL IDENTITY,
        [ServiceId] int NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(2000) NULL,
        CONSTRAINT [PK_ServiceRoles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ServiceRoles_Services_ServiceId] FOREIGN KEY ([ServiceId]) REFERENCES [Services] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [Persons] (
        [Id] int NOT NULL IDENTITY,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [Telephone] nvarchar(50) NOT NULL,
        [UserId] int NULL,
        [ExternalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [CreatedBy] nvarchar(254) NOT NULL,
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [LastUpdatedBy] nvarchar(254) NOT NULL,
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Persons] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Persons_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [OrganisationsConnections] (
        [Id] int NOT NULL IDENTITY,
        [FromOrganisationId] int NOT NULL,
        [FromOrganisationRoleId] int NOT NULL,
        [ToOrganisationId] int NOT NULL,
        [ToOrganisationRoleId] int NOT NULL,
        [ExternalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [CreatedBy] nvarchar(254) NOT NULL,
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [LastUpdatedBy] nvarchar(254) NOT NULL,
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_OrganisationsConnections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganisationsConnections_InterOrganisationRoles_FromOrganisationRoleId] FOREIGN KEY ([FromOrganisationRoleId]) REFERENCES [InterOrganisationRoles] ([Id]),
        CONSTRAINT [FK_OrganisationsConnections_InterOrganisationRoles_ToOrganisationRoleId] FOREIGN KEY ([ToOrganisationRoleId]) REFERENCES [InterOrganisationRoles] ([Id]),
        CONSTRAINT [FK_OrganisationsConnections_Organisations_FromOrganisationId] FOREIGN KEY ([FromOrganisationId]) REFERENCES [Organisations] ([Id]),
        CONSTRAINT [FK_OrganisationsConnections_Organisations_ToOrganisationId] FOREIGN KEY ([ToOrganisationId]) REFERENCES [Organisations] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [PersonOrganisationConnections] (
        [Id] int NOT NULL IDENTITY,
        [JobTitle] nvarchar(50) NULL,
        [OrganisationId] int NOT NULL,
        [OrganisationRoleId] int NOT NULL,
        [PersonId] int NOT NULL,
        [PersonRoleId] int NOT NULL,
        [ExternalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [CreatedBy] nvarchar(254) NOT NULL,
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [LastUpdatedBy] nvarchar(254) NOT NULL,
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_PersonOrganisationConnections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PersonOrganisationConnections_Organisations_OrganisationId] FOREIGN KEY ([OrganisationId]) REFERENCES [Organisations] ([Id]),
        CONSTRAINT [FK_PersonOrganisationConnections_OrganisationToPersonRoles_OrganisationRoleId] FOREIGN KEY ([OrganisationRoleId]) REFERENCES [OrganisationToPersonRoles] ([Id]),
        CONSTRAINT [FK_PersonOrganisationConnections_PersonInOrganisationRoles_PersonRoleId] FOREIGN KEY ([PersonRoleId]) REFERENCES [PersonInOrganisationRoles] ([Id]),
        CONSTRAINT [FK_PersonOrganisationConnections_Persons_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Persons] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [PersonsConnections] (
        [Id] int NOT NULL IDENTITY,
        [FromPersonId] int NOT NULL,
        [FromPersonRoleId] int NOT NULL,
        [ToPersonId] int NOT NULL,
        [ToPersonRoleId] int NOT NULL,
        CONSTRAINT [PK_PersonsConnections] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PersonsConnections_InterPersonRoles_FromPersonRoleId] FOREIGN KEY ([FromPersonRoleId]) REFERENCES [InterPersonRoles] ([Id]),
        CONSTRAINT [FK_PersonsConnections_InterPersonRoles_ToPersonRoleId] FOREIGN KEY ([ToPersonRoleId]) REFERENCES [InterPersonRoles] ([Id]),
        CONSTRAINT [FK_PersonsConnections_Persons_FromPersonId] FOREIGN KEY ([FromPersonId]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_PersonsConnections_Persons_ToPersonId] FOREIGN KEY ([ToPersonId]) REFERENCES [Persons] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE TABLE [Enrolments] (
        [Id] int NOT NULL IDENTITY,
        [ConnectionId] int NOT NULL,
        [ServiceRoleId] int NOT NULL,
        [EnrolmentStatusId] int NOT NULL,
        [ValidFrom] datetimeoffset NULL,
        [ValidTo] datetimeoffset NULL,
        [ExternalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [CreatedBy] nvarchar(254) NOT NULL,
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [LastUpdatedBy] nvarchar(254) NOT NULL,
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_Enrolments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Enrolments_EnrolmentStatuses_EnrolmentStatusId] FOREIGN KEY ([EnrolmentStatusId]) REFERENCES [EnrolmentStatuses] ([Id]),
        CONSTRAINT [FK_Enrolments_PersonOrganisationConnections_ConnectionId] FOREIGN KEY ([ConnectionId]) REFERENCES [PersonOrganisationConnections] ([Id]),
        CONSTRAINT [FK_Enrolments_ServiceRoles_ServiceRoleId] FOREIGN KEY ([ServiceRoleId]) REFERENCES [ServiceRoles] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[EnrolmentStatuses]'))
        SET IDENTITY_INSERT [EnrolmentStatuses] ON;
    EXEC(N'INSERT INTO [EnrolmentStatuses] ([Id], [Name])
    VALUES (0, N''Not Set''),
    (1, N''Enrolled''),
    (2, N''Pending''),
    (3, N''Approved''),
    (4, N''Rejected'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[EnrolmentStatuses]'))
        SET IDENTITY_INSERT [EnrolmentStatuses] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Nations]'))
        SET IDENTITY_INSERT [Nations] ON;
    EXEC(N'INSERT INTO [Nations] ([Id], [Name])
    VALUES (0, N''Not Set''),
    (1, N''England''),
    (2, N''Northern Ireland''),
    (3, N''Scotland''),
    (4, N''Wales'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Nations]'))
        SET IDENTITY_INSERT [Nations] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationToPersonRoles]'))
        SET IDENTITY_INSERT [OrganisationToPersonRoles] ON;
    EXEC(N'INSERT INTO [OrganisationToPersonRoles] ([Id], [Name])
    VALUES (0, N''Not Set''),
    (1, N''Employer'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationToPersonRoles]'))
        SET IDENTITY_INSERT [OrganisationToPersonRoles] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationTypes]'))
        SET IDENTITY_INSERT [OrganisationTypes] ON;
    EXEC(N'INSERT INTO [OrganisationTypes] ([Id], [Name])
    VALUES (0, N''Not Set''),
    (1, N''Limited Company''),
    (2, N''Sole Trader'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationTypes]'))
        SET IDENTITY_INSERT [OrganisationTypes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[PersonInOrganisationRoles]'))
        SET IDENTITY_INSERT [PersonInOrganisationRoles] ON;
    EXEC(N'INSERT INTO [PersonInOrganisationRoles] ([Id], [Name])
    VALUES (0, N''Not Set''),
    (1, N''Admin''),
    (2, N''Employee'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[PersonInOrganisationRoles]'))
        SET IDENTITY_INSERT [PersonInOrganisationRoles] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name') AND [object_id] = OBJECT_ID(N'[Services]'))
        SET IDENTITY_INSERT [Services] ON;
    EXEC(N'INSERT INTO [Services] ([Id], [Description], [Key], [Name])
    VALUES (1, N''Extended Producer Responsibility - Packaging'', N''Packaging'', N''EPR Packaging'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name') AND [object_id] = OBJECT_ID(N'[Services]'))
        SET IDENTITY_INSERT [Services] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] ON;
    EXEC(N'INSERT INTO [ServiceRoles] ([Id], [Description], [Key], [Name], [ServiceId])
    VALUES (1, NULL, N''Packaging.ApprovedPerson'', N''Approved Person'', 1)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] ON;
    EXEC(N'INSERT INTO [ServiceRoles] ([Id], [Description], [Key], [Name], [ServiceId])
    VALUES (2, NULL, N''Packaging.DelegatedPerson'', N''Delegated Person'', 1)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] ON;
    EXEC(N'INSERT INTO [ServiceRoles] ([Id], [Description], [Key], [Name], [ServiceId])
    VALUES (3, NULL, N''Packaging.BasicUser'', N''Basic User'', 1)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_Enrolments_ConnectionId] ON [Enrolments] ([ConnectionId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_Enrolments_EnrolmentStatusId] ON [Enrolments] ([EnrolmentStatusId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_Enrolments_ServiceRoleId] ON [Enrolments] ([ServiceRoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_Organisations_NationId] ON [Organisations] ([NationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_Organisations_OrganisationTypeId] ON [Organisations] ([OrganisationTypeId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_OrganisationsConnections_FromOrganisationId] ON [OrganisationsConnections] ([FromOrganisationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_OrganisationsConnections_FromOrganisationRoleId] ON [OrganisationsConnections] ([FromOrganisationRoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_OrganisationsConnections_ToOrganisationId] ON [OrganisationsConnections] ([ToOrganisationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_OrganisationsConnections_ToOrganisationRoleId] ON [OrganisationsConnections] ([ToOrganisationRoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_PersonOrganisationConnections_OrganisationId] ON [PersonOrganisationConnections] ([OrganisationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_PersonOrganisationConnections_OrganisationRoleId] ON [PersonOrganisationConnections] ([OrganisationRoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_PersonOrganisationConnections_PersonId] ON [PersonOrganisationConnections] ([PersonId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_PersonOrganisationConnections_PersonRoleId] ON [PersonOrganisationConnections] ([PersonRoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Persons_UserId] ON [Persons] ([UserId]) WHERE [UserId] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_PersonsConnections_FromPersonId] ON [PersonsConnections] ([FromPersonId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_PersonsConnections_FromPersonRoleId] ON [PersonsConnections] ([FromPersonRoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_PersonsConnections_ToPersonId] ON [PersonsConnections] ([ToPersonId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_PersonsConnections_ToPersonRoleId] ON [PersonsConnections] ([ToPersonRoleId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    CREATE INDEX [IX_ServiceRoles_ServiceId] ON [ServiceRoles] ([ServiceId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230221151351_Initial')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230221151351_Initial', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230310121027_Add_ReferenceName_Sequence')
BEGIN
    CREATE SEQUENCE [OrganisationReferenceNumber] AS int START WITH 100001 INCREMENT BY 1 NO MINVALUE NO MAXVALUE NO CYCLE;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230310121027_Add_ReferenceName_Sequence')
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Organisations]') AND [c].[name] = N'Town');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Organisations] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Organisations] ALTER COLUMN [Town] nvarchar(70) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230310121027_Add_ReferenceName_Sequence')
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Organisations]') AND [c].[name] = N'ReferenceNumber');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Organisations] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Organisations] ADD DEFAULT (NEXT VALUE FOR OrganisationReferenceNumber) FOR [ReferenceNumber];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230310121027_Add_ReferenceName_Sequence')
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Organisations]') AND [c].[name] = N'Postcode');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Organisations] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Organisations] ALTER COLUMN [Postcode] nvarchar(15) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230310121027_Add_ReferenceName_Sequence')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230310121027_Add_ReferenceName_Sequence', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230321122644_Add_SoftDelete_To_Users')
BEGIN
    ALTER TABLE [Users] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230321122644_Add_SoftDelete_To_Users')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Users_UserId] ON [Users] ([UserId]) WHERE [IsDeleted] = CAST(0 AS bit)');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230321122644_Add_SoftDelete_To_Users')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230321122644_Add_SoftDelete_To_Users', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230323155938_Add_Compliance_Schemes')
BEGIN
    CREATE TABLE [ComplianceSchemes] (
        [Id] int NOT NULL IDENTITY,
        [OrganisationId] int NOT NULL,
        [Name] nvarchar(max) NOT NULL,
        [ExternalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [CreatedBy] nvarchar(254) NOT NULL,
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [LastUpdatedBy] nvarchar(254) NOT NULL,
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_ComplianceSchemes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComplianceSchemes_Organisations_OrganisationId] FOREIGN KEY ([OrganisationId]) REFERENCES [Organisations] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230323155938_Add_Compliance_Schemes')
BEGIN
    CREATE TABLE [SelectedSchemes] (
        [Id] int NOT NULL IDENTITY,
        [OrganisationConnectionId] int NOT NULL,
        [ComplianceSchemeId] int NOT NULL,
        [ExternalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [CreatedBy] nvarchar(254) NOT NULL,
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [LastUpdatedBy] nvarchar(254) NOT NULL,
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (GETUTCDATE()),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_SelectedSchemes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SelectedSchemes_ComplianceSchemes_ComplianceSchemeId] FOREIGN KEY ([ComplianceSchemeId]) REFERENCES [ComplianceSchemes] ([Id]),
        CONSTRAINT [FK_SelectedSchemes_OrganisationsConnections_OrganisationConnectionId] FOREIGN KEY ([OrganisationConnectionId]) REFERENCES [OrganisationsConnections] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230323155938_Add_Compliance_Schemes')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[InterOrganisationRoles]'))
        SET IDENTITY_INSERT [InterOrganisationRoles] ON;
    EXEC(N'INSERT INTO [InterOrganisationRoles] ([Id], [Name])
    VALUES (0, N''Not Set'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[InterOrganisationRoles]'))
        SET IDENTITY_INSERT [InterOrganisationRoles] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230323155938_Add_Compliance_Schemes')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[InterOrganisationRoles]'))
        SET IDENTITY_INSERT [InterOrganisationRoles] ON;
    EXEC(N'INSERT INTO [InterOrganisationRoles] ([Id], [Name])
    VALUES (1, N''Producer'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[InterOrganisationRoles]'))
        SET IDENTITY_INSERT [InterOrganisationRoles] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230323155938_Add_Compliance_Schemes')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[InterOrganisationRoles]'))
        SET IDENTITY_INSERT [InterOrganisationRoles] ON;
    EXEC(N'INSERT INTO [InterOrganisationRoles] ([Id], [Name])
    VALUES (2, N''Compliance Scheme'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[InterOrganisationRoles]'))
        SET IDENTITY_INSERT [InterOrganisationRoles] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230323155938_Add_Compliance_Schemes')
BEGIN
    CREATE INDEX [IX_ComplianceSchemes_OrganisationId] ON [ComplianceSchemes] ([OrganisationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230323155938_Add_Compliance_Schemes')
BEGIN
    CREATE INDEX [IX_SelectedSchemes_ComplianceSchemeId] ON [SelectedSchemes] ([ComplianceSchemeId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230323155938_Add_Compliance_Schemes')
BEGIN
    CREATE INDEX [IX_SelectedSchemes_OrganisationConnectionId] ON [SelectedSchemes] ([OrganisationConnectionId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230323155938_Add_Compliance_Schemes')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230323155938_Add_Compliance_Schemes', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230329155806_Add_Invite_User_Columns')
BEGIN
    DROP INDEX [IX_Users_UserId] ON [Users];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230329155806_Add_Invite_User_Columns')
BEGIN
    ALTER TABLE [Users] ADD [InviteToken] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230329155806_Add_Invite_User_Columns')
BEGIN
    ALTER TABLE [Users] ADD [InvitedBy] nvarchar(254) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230329155806_Add_Invite_User_Columns')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Users_UserId] ON [Users] ([UserId]) WHERE [IsDeleted] = CAST(0 AS bit) AND [UserId] <> CAST(0x0 AS UNIQUEIDENTIFIER)');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230329155806_Add_Invite_User_Columns')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230329155806_Add_Invite_User_Columns', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230331143320_AddInvitedEnrolmentStatus')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[EnrolmentStatuses]'))
        SET IDENTITY_INSERT [EnrolmentStatuses] ON;
    EXEC(N'INSERT INTO [EnrolmentStatuses] ([Id], [Name])
    VALUES (5, N''Invited'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[EnrolmentStatuses]'))
        SET IDENTITY_INSERT [EnrolmentStatuses] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230331143320_AddInvitedEnrolmentStatus')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230331143320_AddInvitedEnrolmentStatus', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SelectedSchemes]') AND [c].[name] = N'CreatedBy');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [SelectedSchemes] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [SelectedSchemes] DROP COLUMN [CreatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SelectedSchemes]') AND [c].[name] = N'LastUpdatedBy');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [SelectedSchemes] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [SelectedSchemes] DROP COLUMN [LastUpdatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Persons]') AND [c].[name] = N'CreatedBy');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Persons] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Persons] DROP COLUMN [CreatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Persons]') AND [c].[name] = N'LastUpdatedBy');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Persons] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Persons] DROP COLUMN [LastUpdatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PersonOrganisationConnections]') AND [c].[name] = N'CreatedBy');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [PersonOrganisationConnections] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [PersonOrganisationConnections] DROP COLUMN [CreatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PersonOrganisationConnections]') AND [c].[name] = N'LastUpdatedBy');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [PersonOrganisationConnections] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [PersonOrganisationConnections] DROP COLUMN [LastUpdatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrganisationsConnections]') AND [c].[name] = N'CreatedBy');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [OrganisationsConnections] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [OrganisationsConnections] DROP COLUMN [CreatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrganisationsConnections]') AND [c].[name] = N'LastUpdatedBy');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [OrganisationsConnections] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [OrganisationsConnections] DROP COLUMN [LastUpdatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Organisations]') AND [c].[name] = N'CreatedBy');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Organisations] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [Organisations] DROP COLUMN [CreatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Organisations]') AND [c].[name] = N'LastUpdatedBy');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Organisations] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [Organisations] DROP COLUMN [LastUpdatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Enrolments]') AND [c].[name] = N'CreatedBy');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Enrolments] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [Enrolments] DROP COLUMN [CreatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Enrolments]') AND [c].[name] = N'LastUpdatedBy');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Enrolments] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [Enrolments] DROP COLUMN [LastUpdatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ComplianceSchemes]') AND [c].[name] = N'CreatedBy');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [ComplianceSchemes] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [ComplianceSchemes] DROP COLUMN [CreatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ComplianceSchemes]') AND [c].[name] = N'LastUpdatedBy');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [ComplianceSchemes] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [ComplianceSchemes] DROP COLUMN [LastUpdatedBy];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SelectedSchemes]') AND [c].[name] = N'LastUpdatedOn');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [SelectedSchemes] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [SelectedSchemes] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [LastUpdatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SelectedSchemes]') AND [c].[name] = N'CreatedOn');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [SelectedSchemes] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [SelectedSchemes] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [CreatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    ALTER TABLE [PersonsConnections] ADD [ExternalId] uniqueidentifier NOT NULL DEFAULT (NEWID());
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    ALTER TABLE [PersonsConnections] ADD [CreatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET());
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    ALTER TABLE [PersonsConnections] ADD [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET());
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    ALTER TABLE [PersonsConnections] ADD [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Persons]') AND [c].[name] = N'LastUpdatedOn');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [Persons] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [Persons] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [LastUpdatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Persons]') AND [c].[name] = N'CreatedOn');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Persons] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [Persons] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [CreatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PersonOrganisationConnections]') AND [c].[name] = N'LastUpdatedOn');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [PersonOrganisationConnections] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [PersonOrganisationConnections] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [LastUpdatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PersonOrganisationConnections]') AND [c].[name] = N'CreatedOn');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [PersonOrganisationConnections] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [PersonOrganisationConnections] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [CreatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrganisationsConnections]') AND [c].[name] = N'LastUpdatedOn');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [OrganisationsConnections] DROP CONSTRAINT [' + @var23 + '];');
    ALTER TABLE [OrganisationsConnections] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [LastUpdatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrganisationsConnections]') AND [c].[name] = N'CreatedOn');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [OrganisationsConnections] DROP CONSTRAINT [' + @var24 + '];');
    ALTER TABLE [OrganisationsConnections] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [CreatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Organisations]') AND [c].[name] = N'LastUpdatedOn');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [Organisations] DROP CONSTRAINT [' + @var25 + '];');
    ALTER TABLE [Organisations] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [LastUpdatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var26 sysname;
    SELECT @var26 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Organisations]') AND [c].[name] = N'CreatedOn');
    IF @var26 IS NOT NULL EXEC(N'ALTER TABLE [Organisations] DROP CONSTRAINT [' + @var26 + '];');
    ALTER TABLE [Organisations] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [CreatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var27 sysname;
    SELECT @var27 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Enrolments]') AND [c].[name] = N'LastUpdatedOn');
    IF @var27 IS NOT NULL EXEC(N'ALTER TABLE [Enrolments] DROP CONSTRAINT [' + @var27 + '];');
    ALTER TABLE [Enrolments] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [LastUpdatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var28 sysname;
    SELECT @var28 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Enrolments]') AND [c].[name] = N'CreatedOn');
    IF @var28 IS NOT NULL EXEC(N'ALTER TABLE [Enrolments] DROP CONSTRAINT [' + @var28 + '];');
    ALTER TABLE [Enrolments] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [CreatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var29 sysname;
    SELECT @var29 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ComplianceSchemes]') AND [c].[name] = N'LastUpdatedOn');
    IF @var29 IS NOT NULL EXEC(N'ALTER TABLE [ComplianceSchemes] DROP CONSTRAINT [' + @var29 + '];');
    ALTER TABLE [ComplianceSchemes] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [LastUpdatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    DECLARE @var30 sysname;
    SELECT @var30 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ComplianceSchemes]') AND [c].[name] = N'CreatedOn');
    IF @var30 IS NOT NULL EXEC(N'ALTER TABLE [ComplianceSchemes] DROP CONSTRAINT [' + @var30 + '];');
    ALTER TABLE [ComplianceSchemes] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [CreatedOn];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] bigint NOT NULL IDENTITY,
        [UserId] uniqueidentifier NULL,
        [OrganisationId] uniqueidentifier NULL,
        [ServiceId] nvarchar(100) NULL,
        [Timestamp] datetimeoffset NOT NULL,
        [Entity] nvarchar(100) NOT NULL,
        [Operation] nvarchar(100) NOT NULL,
        [InternalId] int NULL,
        [ExternalId] uniqueidentifier NULL,
        [OldValues] nvarchar(max) NULL,
        [NewValues] nvarchar(max) NULL,
        [Changes] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    CREATE UNIQUE INDEX [IX_SelectedSchemes_ExternalId] ON [SelectedSchemes] ([ExternalId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    CREATE UNIQUE INDEX [IX_PersonsConnections_ExternalId] ON [PersonsConnections] ([ExternalId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    CREATE UNIQUE INDEX [IX_Persons_ExternalId] ON [Persons] ([ExternalId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    CREATE UNIQUE INDEX [IX_PersonOrganisationConnections_ExternalId] ON [PersonOrganisationConnections] ([ExternalId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    CREATE UNIQUE INDEX [IX_OrganisationsConnections_ExternalId] ON [OrganisationsConnections] ([ExternalId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    CREATE UNIQUE INDEX [IX_Organisations_ExternalId] ON [Organisations] ([ExternalId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    CREATE UNIQUE INDEX [IX_Enrolments_ExternalId] ON [Enrolments] ([ExternalId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    CREATE UNIQUE INDEX [IX_ComplianceSchemes_ExternalId] ON [ComplianceSchemes] ([ExternalId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230405163635_AuditLog_Schema_Changes')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230405163635_AuditLog_Schema_Changes', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230412125353_Add_LaOrganisation_Table')
BEGIN
    CREATE TABLE [LaOrganisations] (
        [Id] int NOT NULL IDENTITY,
        [OrganisationId] int NOT NULL,
        [DistrictCode] nvarchar(10) NOT NULL,
        [ExternalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_LaOrganisations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LaOrganisations_Organisations_OrganisationId] FOREIGN KEY ([OrganisationId]) REFERENCES [Organisations] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230412125353_Add_LaOrganisation_Table')
BEGIN
    CREATE UNIQUE INDEX [IX_LaOrganisations_ExternalId] ON [LaOrganisations] ([ExternalId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230412125353_Add_LaOrganisation_Table')
BEGIN
    CREATE UNIQUE INDEX [IX_LaOrganisations_OrganisationId] ON [LaOrganisations] ([OrganisationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230412125353_Add_LaOrganisation_Table')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230412125353_Add_LaOrganisation_Table', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230427131952_Add_Local_Authority_EndpointCode_Data')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationTypes]'))
        SET IDENTITY_INSERT [OrganisationTypes] ON;
    EXEC(N'INSERT INTO [OrganisationTypes] ([Id], [Name])
    VALUES (3, N''Waste Collection Authority'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationTypes]'))
        SET IDENTITY_INSERT [OrganisationTypes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230427131952_Add_Local_Authority_EndpointCode_Data')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationTypes]'))
        SET IDENTITY_INSERT [OrganisationTypes] ON;
    EXEC(N'INSERT INTO [OrganisationTypes] ([Id], [Name])
    VALUES (4, N''Waste Disposal Authority'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationTypes]'))
        SET IDENTITY_INSERT [OrganisationTypes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230427131952_Add_Local_Authority_EndpointCode_Data')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationTypes]'))
        SET IDENTITY_INSERT [OrganisationTypes] ON;
    EXEC(N'INSERT INTO [OrganisationTypes] ([Id], [Name])
    VALUES (5, N''Waste Collection Authority & Waste Disposal Authority'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationTypes]'))
        SET IDENTITY_INSERT [OrganisationTypes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230427131952_Add_Local_Authority_EndpointCode_Data')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230427131952_Add_Local_Authority_EndpointCode_Data', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230503150607_RemovedLaOrganisations_ExternalId')
BEGIN
    DROP INDEX [IX_LaOrganisations_ExternalId] ON [LaOrganisations];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230503150607_RemovedLaOrganisations_ExternalId')
BEGIN
    DECLARE @var31 sysname;
    SELECT @var31 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LaOrganisations]') AND [c].[name] = N'ExternalId');
    IF @var31 IS NOT NULL EXEC(N'ALTER TABLE [LaOrganisations] DROP CONSTRAINT [' + @var31 + '];');
    ALTER TABLE [LaOrganisations] DROP COLUMN [ExternalId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230503150607_RemovedLaOrganisations_ExternalId')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230503150607_RemovedLaOrganisations_ExternalId', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230505104745_Add_ProducerTypes')
BEGIN
    ALTER TABLE [Organisations] ADD [ProducerTypeId] int NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230505104745_Add_ProducerTypes')
BEGIN
    CREATE TABLE [ProducerTypes] (
        [Id] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_ProducerTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230505104745_Add_ProducerTypes')
BEGIN
    EXEC(N'UPDATE [OrganisationTypes] SET [Name] = N''Companies House Company''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230505104745_Add_ProducerTypes')
BEGIN
    EXEC(N'UPDATE [OrganisationTypes] SET [Name] = N''Non Companies House Company''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230505104745_Add_ProducerTypes')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[ProducerTypes]'))
        SET IDENTITY_INSERT [ProducerTypes] ON;
    EXEC(N'INSERT INTO [ProducerTypes] ([Id], [Name])
    VALUES (0, N''Not Set''),
    (1, N''Partnership''),
    (2, N''Unincorporated body''),
    (3, N''Non-UK organisation''),
    (4, N''Sole trader''),
    (5, N''Other'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[ProducerTypes]'))
        SET IDENTITY_INSERT [ProducerTypes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230505104745_Add_ProducerTypes')
BEGIN
    CREATE INDEX [IX_Organisations_ProducerTypeId] ON [Organisations] ([ProducerTypeId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230505104745_Add_ProducerTypes')
BEGIN
    ALTER TABLE [Organisations] ADD CONSTRAINT [FK_Organisations_ProducerTypes_ProducerTypeId] FOREIGN KEY ([ProducerTypeId]) REFERENCES [ProducerTypes] ([Id]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230505104745_Add_ProducerTypes')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230505104745_Add_ProducerTypes', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230505111110_PersonOrganisationConnection_JobTitle_MaxLength')
BEGIN
    DECLARE @var32 sysname;
    SELECT @var32 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PersonOrganisationConnections]') AND [c].[name] = N'JobTitle');
    IF @var32 IS NOT NULL EXEC(N'ALTER TABLE [PersonOrganisationConnections] DROP CONSTRAINT [' + @var32 + '];');
    ALTER TABLE [PersonOrganisationConnections] ALTER COLUMN [JobTitle] nvarchar(450) NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230505111110_PersonOrganisationConnection_JobTitle_MaxLength')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230505111110_PersonOrganisationConnection_JobTitle_MaxLength', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523154654_Add_Compliance_Scheme_Data')
BEGIN
    ALTER TABLE [ComplianceSchemes] DROP CONSTRAINT [FK_ComplianceSchemes_Organisations_OrganisationId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523154654_Add_Compliance_Scheme_Data')
BEGIN
    DROP INDEX [IX_ComplianceSchemes_OrganisationId] ON [ComplianceSchemes];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523154654_Add_Compliance_Scheme_Data')
BEGIN
    DECLARE @var33 sysname;
    SELECT @var33 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ComplianceSchemes]') AND [c].[name] = N'OrganisationId');
    IF @var33 IS NOT NULL EXEC(N'ALTER TABLE [ComplianceSchemes] DROP CONSTRAINT [' + @var33 + '];');
    ALTER TABLE [ComplianceSchemes] DROP COLUMN [OrganisationId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523154654_Add_Compliance_Scheme_Data')
BEGIN
    ALTER TABLE [ComplianceSchemes] ADD [CompaniesHouseNumber] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523154654_Add_Compliance_Scheme_Data')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompaniesHouseNumber', N'Name') AND [object_id] = OBJECT_ID(N'[ComplianceSchemes]'))
        SET IDENTITY_INSERT [ComplianceSchemes] ON;
    EXEC(N'INSERT INTO [ComplianceSchemes] ([Id], [CompaniesHouseNumber], [Name])
    VALUES (1, N''9100134'', N''Sustain Drinks Packaging Partnership''),
    (2, N''946107'', N''Biffpack (Northern Ireland Environment Agency)''),
    (3, N''946107'', N''Biffpack (Environment Agency)''),
    (4, N''4559478'', N''Comply with Clarity''),
    (5, N''4559478'', N''Comply with Clarity (Northern Ireland Environment Agency)''),
    (6, N''SC331930'', N''Co2 Compliance (Northern Ireland Environment Agency)''),
    (7, N''SC331930'', N''Co2 Compliance (Scottish Environment Protection Agency)''),
    (8, N''5695937'', N''Comply Direct (Environment Agency)''),
    (9, N''5695937'', N''Comply Direct (Northern Ireland Environment Agency)''),
    (10, N''4164355'', N''ComplyPak (Environment Agency)''),
    (11, N''4164355'', N''ComplyPak (Northern Ireland Environment Agency)''),
    (12, N''4713606'', N''Ecosurety (Environment Agency)''),
    (13, N''4713606'', N''Ecosurety (Northern Ireland Environment Agency)''),
    (14, N''SC499366'', N''Ecosurety Scotland''),
    (15, N''5859725'', N''ERP UK Ltd''),
    (16, N''5859725'', N''ERP UK Ltd Northern Ireland Environment Agency''),
    (17, N''4592964'', N''Kite Environmental Solutions''),
    (18, N''4592964'', N''Kite Environmental Solutions (Northern Ireland Environment Agency)''),
    (19, N''NI044560'', N''Nipak Ltd (Northern Ireland Environment Agency)''),
    (20, N''SC300070'', N''Scotpak (Scottish Environment Protection Agency)''),
    (21, N''7699232'', N''Paperpak (Environment Agency)''),
    (22, N''7699232'', N''Paperpak (Northern Ireland Environment Agency)''),
    (23, N''3417947'', N''Veolia (Environment Agency)''),
    (24, N''SC394249'', N''Veolia - Scottish Environment Protection Agency''),
    (25, N''4543366'', N''Recycle Wales''),
    (26, N''SC225789'', N''Recycle-Pak (Northern Ireland Environment Agency)''),
    (27, N''SC225789'', N''Recycle-Pak (Scottish Environment Protection Agency)''),
    (28, N''4748329'', N''Recycling Lives Compliance Services''),
    (29, N''6355083'', N''REPIC''),
    (30, N''6355083'', N''REPIC NI''),
    (31, N''4015442'', N''Smart Comply (Environment Agency)''),
    (32, N''4015442'', N''Smart Comply (Northern Ireland Environment Agency)''),
    (33, N''4835772'', N''Synergy Compliance (Environment Agency)''),
    (34, N''4835772'', N''Synergy Compliance (Northern Ireland Environment Agency)''),
    (35, N''6929701'', N''Ethical Compliance''),
    (36, N''6929701'', N''Pennine-Pack (Environment Agency)''),
    (37, N''6929701'', N''Pennine-Pack (Northern Ireland Environment Agency)''),
    (38, N''3985811'', N''Wastepack (Scottish Environment Protection Agency)''),
    (39, N''3985811'', N''Wastepack UK (Northern Ireland Environment Agency)''),
    (40, N''7688691'', N''Valpak (Environment Agency)''),
    (41, N''7688691'', N''Valpak (Northern Ireland Environment Agency)''),
    (42, N''SC245145'', N''Valpak Scotland Limited (Scottish Environment Protection Agency)'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompaniesHouseNumber', N'Name') AND [object_id] = OBJECT_ID(N'[ComplianceSchemes]'))
        SET IDENTITY_INSERT [ComplianceSchemes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523154654_Add_Compliance_Scheme_Data')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompaniesHouseNumber', N'Name') AND [object_id] = OBJECT_ID(N'[ComplianceSchemes]'))
        SET IDENTITY_INSERT [ComplianceSchemes] ON;
    EXEC(N'INSERT INTO [ComplianceSchemes] ([Id], [CompaniesHouseNumber], [Name])
    VALUES (43, N''2215767'', N''Veolia Environmental Services (Northern Ireland Environment Agency)''),
    (44, N''6043169'', N''Packcare''),
    (45, N''SC174113'', N''Compliance Link (Scottish Environment Protection Agency)''),
    (46, N''SC174113'', N''SWS Compak''),
    (47, N''SC174113'', N''Wastepack (Scottish Environment Protection Agency)''),
    (48, N''4168907'', N''Enpack'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompaniesHouseNumber', N'Name') AND [object_id] = OBJECT_ID(N'[ComplianceSchemes]'))
        SET IDENTITY_INSERT [ComplianceSchemes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230523154654_Add_Compliance_Scheme_Data')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230523154654_Add_Compliance_Scheme_Data', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''09100134''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''00946107''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''00946107''
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04559478''
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04559478''
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''05695937''
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''05695937''
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04164355''
    WHERE [Id] = 10;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04164355''
    WHERE [Id] = 11;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04713606''
    WHERE [Id] = 12;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04713606''
    WHERE [Id] = 13;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''05859725''
    WHERE [Id] = 15;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''05859725''
    WHERE [Id] = 16;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04592964''
    WHERE [Id] = 17;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04592964''
    WHERE [Id] = 18;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''07699232''
    WHERE [Id] = 21;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''07699232''
    WHERE [Id] = 22;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''03417947''
    WHERE [Id] = 23;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04543366''
    WHERE [Id] = 25;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04748329''
    WHERE [Id] = 28;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''06355083''
    WHERE [Id] = 29;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''06355083''
    WHERE [Id] = 30;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04015442''
    WHERE [Id] = 31;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04015442''
    WHERE [Id] = 32;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04835772''
    WHERE [Id] = 33;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04835772''
    WHERE [Id] = 34;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''06929701''
    WHERE [Id] = 35;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''06929701''
    WHERE [Id] = 36;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''06929701''
    WHERE [Id] = 37;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''03985811''
    WHERE [Id] = 38;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''03985811''
    WHERE [Id] = 39;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''07688691''
    WHERE [Id] = 40;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''07688691''
    WHERE [Id] = 41;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''02215767''
    WHERE [Id] = 43;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''06043169''
    WHERE [Id] = 44;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [CompaniesHouseNumber] = N''04168907''
    WHERE [Id] = 48;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230524150612_UpdateCompaniesHouseNumber')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230524150612_UpdateCompaniesHouseNumber', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230607091220_Add_DelegatedPersonEnrolment')
BEGIN
    CREATE TABLE [DelegatedPersonEnrolments] (
        [Id] int NOT NULL IDENTITY,
        [EnrolmentId] int NOT NULL,
        [InviterEnrolmentId] int NOT NULL,
        [RelationshipType] nvarchar(50) NOT NULL,
        [ConsultancyName] nvarchar(160) NULL,
        [ComplianceSchemeName] nvarchar(160) NULL,
        [OtherOrganisationName] nvarchar(160) NULL,
        [OtherRelationshipDescription] nvarchar(160) NULL,
        [InviterDeclaration] nvarchar(450) NULL,
        [InviterDeclarationTime] datetimeoffset NULL,
        [InviteeDeclaration] nvarchar(450) NULL,
        [InviteeDeclarationTime] datetimeoffset NULL,
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_DelegatedPersonEnrolments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DelegatedPersonEnrolments_Enrolments_EnrolmentId] FOREIGN KEY ([EnrolmentId]) REFERENCES [Enrolments] ([Id]),
        CONSTRAINT [FK_DelegatedPersonEnrolments_Enrolments_InviterEnrolmentId] FOREIGN KEY ([InviterEnrolmentId]) REFERENCES [Enrolments] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230607091220_Add_DelegatedPersonEnrolment')
BEGIN
    CREATE INDEX [IX_DelegatedPersonEnrolments_EnrolmentId] ON [DelegatedPersonEnrolments] ([EnrolmentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230607091220_Add_DelegatedPersonEnrolment')
BEGIN
    CREATE UNIQUE INDEX [IX_DelegatedPersonEnrolments_InviterEnrolmentId] ON [DelegatedPersonEnrolments] ([InviterEnrolmentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230607091220_Add_DelegatedPersonEnrolment')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230607091220_Add_DelegatedPersonEnrolment', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    ALTER TABLE [Persons] ADD [RegulatorCommentId] int NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    ALTER TABLE [Organisations] ADD [TransferNationId] int NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    ALTER TABLE [Enrolments] ADD [RegulatorCommentId] int NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    CREATE TABLE [RegulatorComments] (
        [Id] int NOT NULL IDENTITY,
        [RegulatorUserId] int NOT NULL,
        [EnrolmentId] int NOT NULL,
        [RejectedComments] nvarchar(200) NULL,
        [TransferComments] nvarchar(200) NULL,
        [OnHoldComments] nvarchar(200) NULL,
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [ExternalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        CONSTRAINT [PK_RegulatorComments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[EnrolmentStatuses]'))
        SET IDENTITY_INSERT [EnrolmentStatuses] ON;
    EXEC(N'INSERT INTO [EnrolmentStatuses] ([Id], [Name])
    VALUES (6, N''On Hold'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[EnrolmentStatuses]'))
        SET IDENTITY_INSERT [EnrolmentStatuses] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    CREATE INDEX [IX_Persons_RegulatorCommentId] ON [Persons] ([RegulatorCommentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    CREATE INDEX [IX_Organisations_TransferNationId] ON [Organisations] ([TransferNationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Enrolments_RegulatorCommentId] ON [Enrolments] ([RegulatorCommentId]) WHERE [RegulatorCommentId] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    CREATE UNIQUE INDEX [IX_RegulatorComments_ExternalId] ON [RegulatorComments] ([ExternalId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    ALTER TABLE [Enrolments] ADD CONSTRAINT [FK_Enrolments_RegulatorComments_RegulatorCommentId] FOREIGN KEY ([RegulatorCommentId]) REFERENCES [RegulatorComments] ([Id]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    ALTER TABLE [Organisations] ADD CONSTRAINT [FK_Organisations_Nations_TransferNationId] FOREIGN KEY ([TransferNationId]) REFERENCES [Nations] ([Id]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    ALTER TABLE [Persons] ADD CONSTRAINT [FK_Persons_RegulatorComments_RegulatorCommentId] FOREIGN KEY ([RegulatorCommentId]) REFERENCES [RegulatorComments] ([Id]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230615160239_add_regulator_comments_table')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230615160239_add_regulator_comments_table', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    ALTER TABLE [DelegatedPersonEnrolments] DROP CONSTRAINT [FK_DelegatedPersonEnrolments_Enrolments_InviterEnrolmentId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    DROP INDEX [IX_DelegatedPersonEnrolments_EnrolmentId] ON [DelegatedPersonEnrolments];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    DROP INDEX [IX_DelegatedPersonEnrolments_InviterEnrolmentId] ON [DelegatedPersonEnrolments];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    EXEC sp_rename N'[DelegatedPersonEnrolments].[InviterEnrolmentId]', N'NominatorEnrolmentId', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    EXEC sp_rename N'[DelegatedPersonEnrolments].[InviterDeclarationTime]', N'NominatorDeclarationTime', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    EXEC sp_rename N'[DelegatedPersonEnrolments].[InviterDeclaration]', N'NominatorDeclaration', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    EXEC sp_rename N'[DelegatedPersonEnrolments].[InviteeDeclarationTime]', N'NomineeDeclarationTime', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    EXEC sp_rename N'[DelegatedPersonEnrolments].[InviteeDeclaration]', N'NomineeDeclaration', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[EnrolmentStatuses]'))
        SET IDENTITY_INSERT [EnrolmentStatuses] ON;
    EXEC(N'INSERT INTO [EnrolmentStatuses] ([Id], [Name])
    VALUES (7, N''Nominated'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[EnrolmentStatuses]'))
        SET IDENTITY_INSERT [EnrolmentStatuses] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    CREATE UNIQUE INDEX [IX_DelegatedPersonEnrolments_EnrolmentId] ON [DelegatedPersonEnrolments] ([EnrolmentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    CREATE INDEX [IX_DelegatedPersonEnrolments_NominatorEnrolmentId] ON [DelegatedPersonEnrolments] ([NominatorEnrolmentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    ALTER TABLE [DelegatedPersonEnrolments] ADD CONSTRAINT [FK_DelegatedPersonEnrolments_Enrolments_NominatorEnrolmentId] FOREIGN KEY ([NominatorEnrolmentId]) REFERENCES [Enrolments] ([Id]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230619225634_Add_Nominated_EnrolmentStatus')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230619225634_Add_Nominated_EnrolmentStatus', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230622151830_Add_Organisation_Type_For_Regulator')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationTypes]'))
        SET IDENTITY_INSERT [OrganisationTypes] ON;
    EXEC(N'INSERT INTO [OrganisationTypes] ([Id], [Name])
    VALUES (6, N''Regulators'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationTypes]'))
        SET IDENTITY_INSERT [OrganisationTypes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230622151830_Add_Organisation_Type_For_Regulator')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230622151830_Add_Organisation_Type_For_Regulator', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230627112942_Update_Compliance_Scheme_For_Beyondly')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Beyondly''
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230627112942_Update_Compliance_Scheme_For_Beyondly')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230627112942_Update_Compliance_Scheme_For_Beyondly')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230627112942_Update_Compliance_Scheme_For_Beyondly', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230711132949_Add_Regulator_User_Service_Roles')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name') AND [object_id] = OBJECT_ID(N'[Services]'))
        SET IDENTITY_INSERT [Services] ON;
    EXEC(N'INSERT INTO [Services] ([Id], [Description], [Key], [Name])
    VALUES (2, N''Extended Producer Responsibility - Regulating'', N''Regulating'', N''EPR Regulating'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name') AND [object_id] = OBJECT_ID(N'[Services]'))
        SET IDENTITY_INSERT [Services] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230711132949_Add_Regulator_User_Service_Roles')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] ON;
    EXEC(N'INSERT INTO [ServiceRoles] ([Id], [Description], [Key], [Name], [ServiceId])
    VALUES (4, N''Regulator Admin Service Role'', N''Regulator.Admin'', N''Regulator Admin'', 2)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230711132949_Add_Regulator_User_Service_Roles')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] ON;
    EXEC(N'INSERT INTO [ServiceRoles] ([Id], [Description], [Key], [Name], [ServiceId])
    VALUES (5, N''Regulator Basic Service Role'', N''Regulator.Basic'', N''Regulator Basic'', 2)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230711132949_Add_Regulator_User_Service_Roles')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230711132949_Add_Regulator_User_Service_Roles', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Biffpack''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Co2 Compliance''
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''ComplyPak''
    WHERE [Id] = 10;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 11;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Ecosurety (EA)''
    WHERE [Id] = 12;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 13;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 16;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit), [Name] = N''Kite Environmental Solutions''
    WHERE [Id] = 18;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 19;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Scotpak''
    WHERE [Id] = 20;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Paperpak''
    WHERE [Id] = 21;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 22;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Veolia - EA''
    WHERE [Id] = 23;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Veolia (SEPA)''
    WHERE [Id] = 24;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 26;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 27;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 30;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Smart Comply (EA)''
    WHERE [Id] = 31;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 32;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Synergy Compliance''
    WHERE [Id] = 33;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 34;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Pennine-Pack''
    WHERE [Id] = 36;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 37;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 38;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 39;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Valpak''
    WHERE [Id] = 40;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 41;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Valpak Scotland Ltd''
    WHERE [Id] = 42;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 43;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 45;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [IsDeleted] = CAST(1 AS bit)
    WHERE [Id] = 46;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Wastepack''
    WHERE [Id] = 47;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompaniesHouseNumber', N'Name') AND [object_id] = OBJECT_ID(N'[ComplianceSchemes]'))
        SET IDENTITY_INSERT [ComplianceSchemes] ON;
    EXEC(N'INSERT INTO [ComplianceSchemes] ([Id], [CompaniesHouseNumber], [Name])
    VALUES (49, N''SC300070'', N''Leafpak''),
    (50, N''03985811'', N''Wastepack (EA)'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompaniesHouseNumber', N'Name') AND [object_id] = OBJECT_ID(N'[ComplianceSchemes]'))
        SET IDENTITY_INSERT [ComplianceSchemes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230714092052_Update_Compliance_Schemes')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230714092052_Update_Compliance_Schemes', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230731101126_OfflineRegulatorApprovalTables')
BEGIN
    CREATE TABLE [EnrolmentReport] (
        [Id] int NOT NULL IDENTITY,
        [Org Name] nvarchar(50) NULL,
        [PCS or Direct producer?] nvarchar(50) NULL,
        [Compliance Scheme Name] nvarchar(50) NULL,
        [Org Type] nvarchar(50) NULL,
        [Request Type] nvarchar(50) NULL,
        [Submitted Date] date NULL,
        [Organisation ID] nvarchar(50) NULL,
        [Companies House Number] nvarchar(50) NULL,
        [Nation Of Enrolment] nvarchar(50) NULL,
        [Due date] date NULL,
        [AP First Name] nvarchar(50) NULL,
        [AP Last Name] nvarchar(50) NULL,
        [AP Position] nvarchar(50) NULL,
        [AP Contact] nvarchar(50) NULL,
        [AP Email] nvarchar(50) NULL,
        [DP First Name] nvarchar(50) NULL,
        [DP Last Name] nvarchar(50) NULL,
        [DP Position] nvarchar(50) NULL,
        [DP Nature Of Relationship] nvarchar(50) NULL,
        [DP Contact] nvarchar(50) NULL,
        [DP Email] nvarchar(50) NULL,
        [Status] nvarchar(50) NULL,
        [Actioned By] nvarchar(50) NULL,
        [Actioned Date] date NULL,
        [Enrolment Id] nvarchar(50) NULL,
        [Regulator Status] nvarchar(50) NULL,
        [Date] date NULL,
        [Regulator User Name] nvarchar(50) NULL,
        [Regulator Rejection Comments] nvarchar(200) NULL,
        [IsProcessed] bit NULL,
        [ProcessedDate] datetime2 NULL,
        [UploadDate] datetime2 NULL,
        [IsValid] bit NULL,
        [BatchNo] int NULL,
        [ExternalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_EnrolmentReport] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230731101126_OfflineRegulatorApprovalTables')
BEGIN
    CREATE TABLE [OfflineApprovalErrorLog] (
        [Id] int NOT NULL IDENTITY,
        [Error_Number] int NOT NULL,
        [Error_Message] nvarchar(max) NOT NULL,
        [Error_Line] int NOT NULL,
        [Error_Procedure] nvarchar(max) NOT NULL,
        [Error_Severity] int NOT NULL,
        [Error_State] int NOT NULL,
        [Error_Date] datetime2 NOT NULL,
        [ExternalId] uniqueidentifier NOT NULL DEFAULT (NEWID()),
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_OfflineApprovalErrorLog] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230731101126_OfflineRegulatorApprovalTables')
BEGIN
    CREATE UNIQUE INDEX [IX_EnrolmentReport_ExternalId] ON [EnrolmentReport] ([ExternalId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230731101126_OfflineRegulatorApprovalTables')
BEGIN
    CREATE UNIQUE INDEX [IX_OfflineApprovalErrorLog_ExternalId] ON [OfflineApprovalErrorLog] ([ExternalId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230731101126_OfflineRegulatorApprovalTables')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230731101126_OfflineRegulatorApprovalTables', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230804150946_Add_RegulatorComments_Relationships')
BEGIN
    ALTER TABLE [Enrolments] DROP CONSTRAINT [FK_Enrolments_RegulatorComments_RegulatorCommentId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230804150946_Add_RegulatorComments_Relationships')
BEGIN
    ALTER TABLE [Persons] DROP CONSTRAINT [FK_Persons_RegulatorComments_RegulatorCommentId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230804150946_Add_RegulatorComments_Relationships')
BEGIN
    DROP INDEX [IX_Persons_RegulatorCommentId] ON [Persons];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230804150946_Add_RegulatorComments_Relationships')
BEGIN
    DROP INDEX [IX_Enrolments_RegulatorCommentId] ON [Enrolments];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230804150946_Add_RegulatorComments_Relationships')
BEGIN
    DECLARE @var34 sysname;
    SELECT @var34 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Persons]') AND [c].[name] = N'RegulatorCommentId');
    IF @var34 IS NOT NULL EXEC(N'ALTER TABLE [Persons] DROP CONSTRAINT [' + @var34 + '];');
    ALTER TABLE [Persons] DROP COLUMN [RegulatorCommentId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230804150946_Add_RegulatorComments_Relationships')
BEGIN
    DECLARE @var35 sysname;
    SELECT @var35 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Enrolments]') AND [c].[name] = N'RegulatorCommentId');
    IF @var35 IS NOT NULL EXEC(N'ALTER TABLE [Enrolments] DROP CONSTRAINT [' + @var35 + '];');
    ALTER TABLE [Enrolments] DROP COLUMN [RegulatorCommentId];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230804150946_Add_RegulatorComments_Relationships')
BEGIN
    EXEC sp_rename N'[RegulatorComments].[RegulatorUserId]', N'PersonId', N'COLUMN';
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230804150946_Add_RegulatorComments_Relationships')
BEGIN
    CREATE INDEX [IX_RegulatorComments_EnrolmentId] ON [RegulatorComments] ([EnrolmentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230804150946_Add_RegulatorComments_Relationships')
BEGIN
    CREATE INDEX [IX_RegulatorComments_PersonId] ON [RegulatorComments] ([PersonId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230804150946_Add_RegulatorComments_Relationships')
BEGIN
    ALTER TABLE [RegulatorComments] ADD CONSTRAINT [FK_RegulatorComments_Enrolments_EnrolmentId] FOREIGN KEY ([EnrolmentId]) REFERENCES [Enrolments] ([Id]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230804150946_Add_RegulatorComments_Relationships')
BEGIN
    ALTER TABLE [RegulatorComments] ADD CONSTRAINT [FK_RegulatorComments_Persons_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Persons] ([Id]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230804150946_Add_RegulatorComments_Relationships')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230804150946_Add_RegulatorComments_Relationships', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230809120104_Add_LaPayment_User_Service_Roles')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name') AND [object_id] = OBJECT_ID(N'[Services]'))
        SET IDENTITY_INSERT [Services] ON;
    EXEC(N'INSERT INTO [Services] ([Id], [Description], [Key], [Name])
    VALUES (3, N''Local Authority Payment Service'', N''LaPayment'', N''Local Authority Payment Service'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name') AND [object_id] = OBJECT_ID(N'[Services]'))
        SET IDENTITY_INSERT [Services] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230809120104_Add_LaPayment_User_Service_Roles')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] ON;
    EXEC(N'INSERT INTO [ServiceRoles] ([Id], [Description], [Key], [Name], [ServiceId])
    VALUES (6, NULL, N''LaPayment.UserAdministrator'', N''User Administrator'', 3)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230809120104_Add_LaPayment_User_Service_Roles')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] ON;
    EXEC(N'INSERT INTO [ServiceRoles] ([Id], [Description], [Key], [Name], [ServiceId])
    VALUES (7, NULL, N''LaPayment.BasicUser'', N''Basic User'', 3)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Description', N'Key', N'Name', N'ServiceId') AND [object_id] = OBJECT_ID(N'[ServiceRoles]'))
        SET IDENTITY_INSERT [ServiceRoles] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230809120104_Add_LaPayment_User_Service_Roles')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230809120104_Add_LaPayment_User_Service_Roles', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230811151019_Add_Organisations_CompaniesHouse_Index')
BEGIN
    EXEC(N'CREATE INDEX [IX_Organisations_CompaniesHouseNumber] ON [Organisations] ([CompaniesHouseNumber]) WHERE [CompaniesHouseNumber] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230811151019_Add_Organisations_CompaniesHouse_Index')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230811151019_Add_Organisations_CompaniesHouse_Index', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230913103433_Add_ComplianceSchemeMemberRemoval_Tables')
BEGIN
    CREATE TABLE [ComplianceSchemeMemberRemovalAuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [SchemeOrganisationId] int NOT NULL,
        [MemberOrganisationId] int NOT NULL,
        [ComplianceSchemeId] int NOT NULL,
        [RemovedBy] uniqueidentifier NOT NULL,
        [ReasonDescription] nvarchar(2000) NULL,
        CONSTRAINT [PK_ComplianceSchemeMemberRemovalAuditLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComplianceSchemeMemberRemovalAuditLogs_ComplianceSchemes_ComplianceSchemeId] FOREIGN KEY ([ComplianceSchemeId]) REFERENCES [ComplianceSchemes] ([Id]),
        CONSTRAINT [FK_ComplianceSchemeMemberRemovalAuditLogs_Organisations_MemberOrganisationId] FOREIGN KEY ([MemberOrganisationId]) REFERENCES [Organisations] ([Id]),
        CONSTRAINT [FK_ComplianceSchemeMemberRemovalAuditLogs_Organisations_SchemeOrganisationId] FOREIGN KEY ([SchemeOrganisationId]) REFERENCES [Organisations] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230913103433_Add_ComplianceSchemeMemberRemoval_Tables')
BEGIN
    CREATE TABLE [ComplianceSchemeMemberRemovalReasons] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(100) NOT NULL,
        [Name] nvarchar(2000) NOT NULL,
        [RequiresReason] bit NOT NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_ComplianceSchemeMemberRemovalReasons] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230913103433_Add_ComplianceSchemeMemberRemoval_Tables')
BEGIN
    CREATE TABLE [ComplianceSchemeMemberRemovalAuditLogsReasons] (
        [Id] int NOT NULL IDENTITY,
        [AuditLogId] int NOT NULL,
        [ReasonId] int NOT NULL,
        CONSTRAINT [PK_ComplianceSchemeMemberRemovalAuditLogsReasons] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ComplianceSchemeMemberRemovalAuditLogsReasons_ComplianceSchemeMemberRemovalAuditLogs_AuditLogId] FOREIGN KEY ([AuditLogId]) REFERENCES [ComplianceSchemeMemberRemovalAuditLogs] ([Id]),
        CONSTRAINT [FK_ComplianceSchemeMemberRemovalAuditLogsReasons_ComplianceSchemeMemberRemovalReasons_ReasonId] FOREIGN KEY ([ReasonId]) REFERENCES [ComplianceSchemeMemberRemovalReasons] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230913103433_Add_ComplianceSchemeMemberRemoval_Tables')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'DisplayOrder', N'IsActive', N'Name', N'RequiresReason') AND [object_id] = OBJECT_ID(N'[ComplianceSchemeMemberRemovalReasons]'))
        SET IDENTITY_INSERT [ComplianceSchemeMemberRemovalReasons] ON;
    EXEC(N'INSERT INTO [ComplianceSchemeMemberRemovalReasons] ([Id], [Code], [DisplayOrder], [IsActive], [Name], [RequiresReason])
    VALUES (1, N''A'', 1, CAST(0 AS bit), N''It won''''t be able to give us its organisation details or packaging data by the deadline'', CAST(0 AS bit)),
    (2, N''B'', 2, CAST(0 AS bit), N''The amount of packaging it handled in the threshold calculation year is less than or equal to 25 tonnes'', CAST(0 AS bit)),
    (3, N''C'', 3, CAST(0 AS bit), N''It has ended its membership with this scheme'', CAST(0 AS bit)),
    (4, N''D'', 4, CAST(0 AS bit), N''It has merged with another company'', CAST(0 AS bit)),
    (5, N''E'', 5, CAST(0 AS bit), N''It is no longer in operation'', CAST(1 AS bit)),
    (6, N''F'', 6, CAST(0 AS bit), N''It has gone into administration'', CAST(0 AS bit)),
    (7, N''G'', 7, CAST(0 AS bit), N''It is now a subsidiary of another company'', CAST(0 AS bit)),
    (8, N''H'', 8, CAST(0 AS bit), N''Its turnover in the last financial year before the relevant date is less than or equal to �1 million'', CAST(0 AS bit)),
    (9, N''I'', 9, CAST(0 AS bit), N''We''''ve ended its membership with this scheme'', CAST(0 AS bit)),
    (10, N''J'', 10, CAST(0 AS bit), N''None of the above'', CAST(1 AS bit))');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'DisplayOrder', N'IsActive', N'Name', N'RequiresReason') AND [object_id] = OBJECT_ID(N'[ComplianceSchemeMemberRemovalReasons]'))
        SET IDENTITY_INSERT [ComplianceSchemeMemberRemovalReasons] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230913103433_Add_ComplianceSchemeMemberRemoval_Tables')
BEGIN
    CREATE INDEX [IX_ComplianceSchemeMemberRemovalAuditLogs_ComplianceSchemeId] ON [ComplianceSchemeMemberRemovalAuditLogs] ([ComplianceSchemeId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230913103433_Add_ComplianceSchemeMemberRemoval_Tables')
BEGIN
    CREATE INDEX [IX_ComplianceSchemeMemberRemovalAuditLogs_MemberOrganisationId] ON [ComplianceSchemeMemberRemovalAuditLogs] ([MemberOrganisationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230913103433_Add_ComplianceSchemeMemberRemoval_Tables')
BEGIN
    CREATE INDEX [IX_ComplianceSchemeMemberRemovalAuditLogs_SchemeOrganisationId] ON [ComplianceSchemeMemberRemovalAuditLogs] ([SchemeOrganisationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230913103433_Add_ComplianceSchemeMemberRemoval_Tables')
BEGIN
    CREATE INDEX [IX_ComplianceSchemeMemberRemovalAuditLogsReasons_AuditLogId] ON [ComplianceSchemeMemberRemovalAuditLogsReasons] ([AuditLogId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230913103433_Add_ComplianceSchemeMemberRemoval_Tables')
BEGIN
    CREATE INDEX [IX_ComplianceSchemeMemberRemovalAuditLogsReasons_ReasonId] ON [ComplianceSchemeMemberRemovalAuditLogsReasons] ([ReasonId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230913103433_Add_ComplianceSchemeMemberRemoval_Tables')
BEGIN
    CREATE UNIQUE INDEX [IX_ComplianceSchemeMemberRemovalReasons_Code] ON [ComplianceSchemeMemberRemovalReasons] ([Code]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230913103433_Add_ComplianceSchemeMemberRemoval_Tables')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230913103433_Add_ComplianceSchemeMemberRemoval_Tables', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    ALTER TABLE [ComplianceSchemes] ADD [NationId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Co2 Compliance (SEPA)'', [NationId] = 3
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 10;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 12;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Ecosurety Scotland (SEPA)'', [NationId] = 3
    WHERE [Id] = 14;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 15;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 17;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Scotpak (SEPA)'', [NationId] = 3
    WHERE [Id] = 20;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 21;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Veolia (EA)'', [NationId] = 1
    WHERE [Id] = 23;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 3
    WHERE [Id] = 24;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Recycle Wales (NRW)'', [NationId] = 4
    WHERE [Id] = 25;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 28;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 29;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 31;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 33;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 35;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 36;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 40;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Valpak Scotland Ltd (SEPA)'', [NationId] = 3
    WHERE [Id] = 42;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 44;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [Name] = N''Wastepack GB (SEPA)'', [NationId] = 3
    WHERE [Id] = 47;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 48;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 2
    WHERE [Id] = 49;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = 1
    WHERE [Id] = 50;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    CREATE INDEX [IX_ComplianceSchemes_NationId] ON [ComplianceSchemes] ([NationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    ALTER TABLE [ComplianceSchemes] ADD CONSTRAINT [FK_ComplianceSchemes_Nations_NationId] FOREIGN KEY ([NationId]) REFERENCES [Nations] ([Id]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230918102845_Add_NationId_To_ComplianceScheme_And_Rename', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    DECLARE @var36 sysname;
    SELECT @var36 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ComplianceSchemes]') AND [c].[name] = N'NationId');
    IF @var36 IS NOT NULL EXEC(N'ALTER TABLE [ComplianceSchemes] DROP CONSTRAINT [' + @var36 + '];');
    ALTER TABLE [ComplianceSchemes] ALTER COLUMN [NationId] int NULL;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 11;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 13;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 16;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 18;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 19;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 22;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 26;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 27;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 30;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 32;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 34;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 37;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 38;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 39;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 41;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 43;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 45;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemes] SET [NationId] = NULL
    WHERE [Id] = 46;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230918125935_Change_ComplianceScheme_NationId_To_Nullable', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230919154838_Add_New_Compliance_Schemes')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompaniesHouseNumber', N'Name', N'NationId') AND [object_id] = OBJECT_ID(N'[ComplianceSchemes]'))
        SET IDENTITY_INSERT [ComplianceSchemes] ON;
    EXEC(N'INSERT INTO [ComplianceSchemes] ([Id], [CompaniesHouseNumber], [Name], [NationId])
    VALUES (51, N''05695937'', N''Beyondly (NIEA)'', 2),
    (52, N''00946107'', N''BiffPack (NIEA)'', 2),
    (53, N''04559478'', N''Comply with Clarity (NIEA)'', 2),
    (54, N''SC331930'', N''Co2 Compliance (NIEA)'', 2),
    (55, N''04164355'', N''ComplyPak (NIEA)'', 2),
    (56, N''04713606'', N''Ecosurety (NIEA)'', 2),
    (57, N''05859725'', N''ERP (NIEA)'', 2),
    (58, N''04592964'', N''Kite Environmental Solutions (NIEA)'', 2),
    (59, N''SC300070'', N''Leafpak (NIEA)'', 2),
    (60, N''07699232'', N''Paperpak (NIEA)'', 2),
    (61, N''03417947'', N''Veolia (NIEA)'', 2),
    (62, N''04748329'', N''Recycling Lives Compliance Services (NIEA)'', 2),
    (63, N''06355083'', N''REPIC (NIEA)'', 2),
    (64, N''04015442'', N''Smart Comply (NIEA)'', 2),
    (65, N''04835772'', N''Synergy Compliance (Northern Ireland Environment Agency)'', 2),
    (66, N''06929701'', N''Pennine-Pack (Northern Ireland Environment Agency)'', 2),
    (67, N''03985811'', N''Wastepack UK (NIEA)'', 2),
    (68, N''07688691'', N''Valpak (NIEA)'', 2)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CompaniesHouseNumber', N'Name', N'NationId') AND [object_id] = OBJECT_ID(N'[ComplianceSchemes]'))
        SET IDENTITY_INSERT [ComplianceSchemes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20230919154838_Add_New_Compliance_Schemes')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230919154838_Add_New_Compliance_Schemes', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231017110211_Update_ComplianceSchemeMemberRemovalAuditLogs_RemovedOn')
BEGIN
    ALTER TABLE [ComplianceSchemeMemberRemovalAuditLogs] ADD [RemovedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET());
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231017110211_Update_ComplianceSchemeMemberRemovalAuditLogs_RemovedOn')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231017110211_Update_ComplianceSchemeMemberRemovalAuditLogs_RemovedOn', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231024131700_Update_ComplianceSchemeMemberRemovalReasons_Table')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemeMemberRemovalReasons] SET [IsActive] = CAST(1 AS bit)
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231024131700_Update_ComplianceSchemeMemberRemovalReasons_Table')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemeMemberRemovalReasons] SET [IsActive] = CAST(1 AS bit)
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231024131700_Update_ComplianceSchemeMemberRemovalReasons_Table')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemeMemberRemovalReasons] SET [IsActive] = CAST(1 AS bit)
    WHERE [Id] = 3;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231024131700_Update_ComplianceSchemeMemberRemovalReasons_Table')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemeMemberRemovalReasons] SET [IsActive] = CAST(1 AS bit)
    WHERE [Id] = 4;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231024131700_Update_ComplianceSchemeMemberRemovalReasons_Table')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemeMemberRemovalReasons] SET [IsActive] = CAST(1 AS bit)
    WHERE [Id] = 5;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231024131700_Update_ComplianceSchemeMemberRemovalReasons_Table')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemeMemberRemovalReasons] SET [IsActive] = CAST(1 AS bit)
    WHERE [Id] = 6;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231024131700_Update_ComplianceSchemeMemberRemovalReasons_Table')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemeMemberRemovalReasons] SET [IsActive] = CAST(1 AS bit)
    WHERE [Id] = 7;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231024131700_Update_ComplianceSchemeMemberRemovalReasons_Table')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemeMemberRemovalReasons] SET [IsActive] = CAST(1 AS bit), [Name] = N''Its turnover in the last financial year before the relevant date is less than or equal to £1 million''
    WHERE [Id] = 8;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231024131700_Update_ComplianceSchemeMemberRemovalReasons_Table')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemeMemberRemovalReasons] SET [IsActive] = CAST(1 AS bit)
    WHERE [Id] = 9;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231024131700_Update_ComplianceSchemeMemberRemovalReasons_Table')
BEGIN
    EXEC(N'UPDATE [ComplianceSchemeMemberRemovalReasons] SET [IsActive] = CAST(1 AS bit)
    WHERE [Id] = 10;
    SELECT @@ROWCOUNT');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231024131700_Update_ComplianceSchemeMemberRemovalReasons_Table')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231024131700_Update_ComplianceSchemeMemberRemovalReasons_Table', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231127153828_Search_Organisations_By_SearchTerm_Query_Index')
BEGIN
    EXEC(N'CREATE INDEX [IX_Name_ReferenceNumber_NationId] ON [Organisations] ([Name], [ReferenceNumber], [NationId]) WHERE [OrganisationTypeId] <> 6 AND [IsDeleted] = CAST(0 AS bit)');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20231127153828_Search_Organisations_By_SearchTerm_Query_Index')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231127153828_Search_Organisations_By_SearchTerm_Query_Index', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240404163942_Add_Organisations_ReferenceNumber_Index')
BEGIN
    EXEC(N'CREATE INDEX [IX_Organisations_ReferenceNumber] ON [Organisations] ([ReferenceNumber]) WHERE [IsDeleted] = CAST(0 AS bit)');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240404163942_Add_Organisations_ReferenceNumber_Index')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240404163942_Add_Organisations_ReferenceNumber_Index', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240405160955_Add_Approved_Person_Enrolment_Table')
BEGIN
    CREATE TABLE [ApprovedPersonEnrolments] (
        [Id] int NOT NULL IDENTITY,
        [EnrolmentId] int NOT NULL,
        [NomineeDeclaration] nvarchar(450) NULL,
        [NomineeDeclarationTime] datetimeoffset NOT NULL,
        [CreatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [LastUpdatedOn] datetimeoffset NOT NULL DEFAULT (SYSDATETIMEOFFSET()),
        [IsDeleted] bit NOT NULL DEFAULT CAST(0 AS bit),
        CONSTRAINT [PK_ApprovedPersonEnrolments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ApprovedPersonEnrolments_Enrolments_EnrolmentId] FOREIGN KEY ([EnrolmentId]) REFERENCES [Enrolments] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240405160955_Add_Approved_Person_Enrolment_Table')
BEGIN
    CREATE UNIQUE INDEX [IX_ApprovedPersonEnrolments_EnrolmentId] ON [ApprovedPersonEnrolments] ([EnrolmentId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240405160955_Add_Approved_Person_Enrolment_Table')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240405160955_Add_Approved_Person_Enrolment_Table', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240709131348_Add_OrganisationRelationshipType_OrganisationRelationships')
BEGIN
    CREATE TABLE [OrganisationRelationshipTypes] (
        [Id] int NOT NULL,
        [Name] nvarchar(100) NULL,
        CONSTRAINT [PK_OrganisationRelationshipTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240709131348_Add_OrganisationRelationshipType_OrganisationRelationships')
BEGIN
    CREATE TABLE [OrganisationRelationships] (
        [Id] int NOT NULL IDENTITY,
        [FirstOrganisationId] int NOT NULL,
        [SecondOrganisationId] int NOT NULL,
        [OrganisationRelationshipTypeId] int NOT NULL,
        [RelationFromDate] datetime2 NOT NULL,
        [RelationToDate] datetime2 NULL,
        [RelationExpiryReason] nvarchar(max) NULL,
        [CreatedOn] datetime2 NOT NULL,
        [LastUpdatedById] int NOT NULL,
        [LastUpdatedOn] datetime2 NOT NULL,
        [LastUpdatedByOrganisationId] int NOT NULL,
        CONSTRAINT [PK_OrganisationRelationships] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_OrganisationRelationships_OrganisationRelationshipTypes_OrganisationRelationshipTypeId] FOREIGN KEY ([OrganisationRelationshipTypeId]) REFERENCES [OrganisationRelationshipTypes] ([Id]),
        CONSTRAINT [FK_OrganisationRelationships_Organisations_FirstOrganisationId] FOREIGN KEY ([FirstOrganisationId]) REFERENCES [Organisations] ([Id]),
        CONSTRAINT [FK_OrganisationRelationships_Users_LastUpdatedById] FOREIGN KEY ([LastUpdatedById]) REFERENCES [Users] ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240709131348_Add_OrganisationRelationshipType_OrganisationRelationships')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationRelationshipTypes]'))
        SET IDENTITY_INSERT [OrganisationRelationshipTypes] ON;
    EXEC(N'INSERT INTO [OrganisationRelationshipTypes] ([Id], [Name])
    VALUES (0, N''Not Set''),
    (1, N''Parent''),
    (2, N''Holding''),
    (3, N''Subsidary'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationRelationshipTypes]'))
        SET IDENTITY_INSERT [OrganisationRelationshipTypes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240709131348_Add_OrganisationRelationshipType_OrganisationRelationships')
BEGIN
    CREATE INDEX [IX_OrganisationRelationships_FirstOrganisationId] ON [OrganisationRelationships] ([FirstOrganisationId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240709131348_Add_OrganisationRelationshipType_OrganisationRelationships')
BEGIN
    CREATE INDEX [IX_OrganisationRelationships_LastUpdatedById] ON [OrganisationRelationships] ([LastUpdatedById]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240709131348_Add_OrganisationRelationshipType_OrganisationRelationships')
BEGIN
    CREATE INDEX [IX_OrganisationRelationships_OrganisationRelationshipTypeId] ON [OrganisationRelationships] ([OrganisationRelationshipTypeId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240709131348_Add_OrganisationRelationshipType_OrganisationRelationships')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240709131348_Add_OrganisationRelationshipType_OrganisationRelationships', N'6.0.15');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240718083252_OrganisationRegistrationType_Table_UpdateRelationships')
BEGIN
    DECLARE @var37 sysname;
    SELECT @var37 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[OrganisationRelationships]') AND [c].[name] = N'RelationFromDate');
    IF @var37 IS NOT NULL EXEC(N'ALTER TABLE [OrganisationRelationships] DROP CONSTRAINT [' + @var37 + '];');
    ALTER TABLE [OrganisationRelationships] ADD DEFAULT (SYSDATETIMEOFFSET()) FOR [RelationFromDate];
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240718083252_OrganisationRegistrationType_Table_UpdateRelationships')
BEGIN
    ALTER TABLE [OrganisationRelationships] ADD [OrganisationRegistrationTypeId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240718083252_OrganisationRegistrationType_Table_UpdateRelationships')
BEGIN
    CREATE TABLE [OrganisationRegistrationTypes] (
        [Id] int NOT NULL,
        [Name] nvarchar(100) NULL,
        [Key] nvarchar(10) NULL,
        CONSTRAINT [PK_OrganisationRegistrationTypes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240718083252_OrganisationRegistrationType_Table_UpdateRelationships')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Key', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationRegistrationTypes]'))
        SET IDENTITY_INSERT [OrganisationRegistrationTypes] ON;
    EXEC(N'INSERT INTO [OrganisationRegistrationTypes] ([Id], [Key], [Name])
    VALUES (0, NULL, N''Not Set'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Key', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationRegistrationTypes]'))
        SET IDENTITY_INSERT [OrganisationRegistrationTypes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240718083252_OrganisationRegistrationType_Table_UpdateRelationships')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Key', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationRegistrationTypes]'))
        SET IDENTITY_INSERT [OrganisationRegistrationTypes] ON;
    EXEC(N'INSERT INTO [OrganisationRegistrationTypes] ([Id], [Key], [Name])
    VALUES (1, NULL, N''Group'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Key', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationRegistrationTypes]'))
        SET IDENTITY_INSERT [OrganisationRegistrationTypes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240718083252_OrganisationRegistrationType_Table_UpdateRelationships')
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Key', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationRegistrationTypes]'))
        SET IDENTITY_INSERT [OrganisationRegistrationTypes] ON;
    EXEC(N'INSERT INTO [OrganisationRegistrationTypes] ([Id], [Key], [Name])
    VALUES (2, NULL, N''Individual'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Key', N'Name') AND [object_id] = OBJECT_ID(N'[OrganisationRegistrationTypes]'))
        SET IDENTITY_INSERT [OrganisationRegistrationTypes] OFF;
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240718083252_OrganisationRegistrationType_Table_UpdateRelationships')
BEGIN
    CREATE INDEX [IX_OrganisationRelationships_OrganisationRegistrationTypeId] ON [OrganisationRelationships] ([OrganisationRegistrationTypeId]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240718083252_OrganisationRegistrationType_Table_UpdateRelationships')
BEGIN
    ALTER TABLE [OrganisationRelationships] ADD CONSTRAINT [FK_OrganisationRelationships_OrganisationRegistrationTypes_OrganisationRegistrationTypeId] FOREIGN KEY ([OrganisationRegistrationTypeId]) REFERENCES [OrganisationRegistrationTypes] ([Id]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240718083252_OrganisationRegistrationType_Table_UpdateRelationships')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240718083252_OrganisationRegistrationType_Table_UpdateRelationships', N'6.0.15');
END;
GO

COMMIT;
GO

