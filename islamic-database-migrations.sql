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
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [ProfileImageUrl] nvarchar(max) NULL,
    [Bio] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [Tracks] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Tracks] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RefreshTokens] (
    [Id] int NOT NULL IDENTITY,
    [Token] nvarchar(max) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsRevoked] bit NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Certificates] (
    [Id] int NOT NULL IDENTITY,
    [StudentId] nvarchar(450) NOT NULL,
    [TrackId] int NOT NULL,
    [IssuedAt] datetime2 NOT NULL,
    [CertificateUrl] nvarchar(max) NULL,
    CONSTRAINT [PK_Certificates] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Certificates_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Certificates_Tracks_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [Tracks] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [InteractiveActivities] (
    [Id] int NOT NULL IDENTITY,
    [TrackId] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Date] datetime2 NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [Capacity] int NOT NULL,
    [Status] int NOT NULL,
    [MeetingUrl] nvarchar(max) NULL,
    CONSTRAINT [PK_InteractiveActivities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InteractiveActivities_Tracks_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [Tracks] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Lessons] (
    [Id] int NOT NULL IDENTITY,
    [TrackId] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Order] int NOT NULL,
    [LessonType] int NOT NULL,
    [ContentUrl] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Lessons] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Lessons_Tracks_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [Tracks] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [TrackEnrollments] (
    [Id] int NOT NULL IDENTITY,
    [StudentId] nvarchar(450) NOT NULL,
    [TrackId] int NOT NULL,
    [EnrolledAt] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [CompletedAt] datetime2 NULL,
    CONSTRAINT [PK_TrackEnrollments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TrackEnrollments_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TrackEnrollments_Tracks_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [Tracks] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [TrackInstructors] (
    [UserId] nvarchar(450) NOT NULL,
    [TrackId] int NOT NULL,
    CONSTRAINT [PK_TrackInstructors] PRIMARY KEY ([UserId], [TrackId]),
    CONSTRAINT [FK_TrackInstructors_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TrackInstructors_Tracks_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [Tracks] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [TrackPackages] (
    [Id] int NOT NULL IDENTITY,
    [TrackId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Price] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_TrackPackages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TrackPackages_Tracks_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [Tracks] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Workshops] (
    [Id] int NOT NULL IDENTITY,
    [TrackId] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Place] nvarchar(max) NOT NULL,
    [Date] datetime2 NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [Capacity] int NOT NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_Workshops] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Workshops_Tracks_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [Tracks] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [InteractiveBookings] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [InteractiveActivityId] int NOT NULL,
    [BookedAt] datetime2 NOT NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_InteractiveBookings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InteractiveBookings_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_InteractiveBookings_InteractiveActivities_InteractiveActivityId] FOREIGN KEY ([InteractiveActivityId]) REFERENCES [InteractiveActivities] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Assignments] (
    [Id] int NOT NULL IDENTITY,
    [LessonId] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [DueDate] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Assignments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Assignments_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [LessonProgresses] (
    [Id] int NOT NULL IDENTITY,
    [StudentId] nvarchar(450) NOT NULL,
    [LessonId] int NOT NULL,
    [IsCompleted] bit NOT NULL,
    [CompletedAt] datetime2 NULL,
    CONSTRAINT [PK_LessonProgresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LessonProgresses_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LessonProgresses_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [TrackPackageBookings] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [TrackPackageId] int NOT NULL,
    [BookedAt] datetime2 NOT NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_TrackPackageBookings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TrackPackageBookings_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TrackPackageBookings_TrackPackages_TrackPackageId] FOREIGN KEY ([TrackPackageId]) REFERENCES [TrackPackages] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [WorkshopBookings] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [WorkshopId] int NOT NULL,
    [BookedAt] datetime2 NOT NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_WorkshopBookings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkshopBookings_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_WorkshopBookings_Workshops_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshops] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AssignmentSubmissions] (
    [Id] int NOT NULL IDENTITY,
    [AssignmentId] int NOT NULL,
    [StudentId] nvarchar(450) NOT NULL,
    [SubmittedAt] datetime2 NOT NULL,
    [Content] nvarchar(max) NULL,
    [FileUrl] nvarchar(max) NULL,
    [Grade] decimal(5,2) NULL,
    [Feedback] nvarchar(max) NULL,
    CONSTRAINT [PK_AssignmentSubmissions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AssignmentSubmissions_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AssignmentSubmissions_Assignments_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [Assignments] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_Assignments_LessonId] ON [Assignments] ([LessonId]);

CREATE INDEX [IX_AssignmentSubmissions_AssignmentId] ON [AssignmentSubmissions] ([AssignmentId]);

CREATE INDEX [IX_AssignmentSubmissions_StudentId] ON [AssignmentSubmissions] ([StudentId]);

CREATE INDEX [IX_Certificates_StudentId] ON [Certificates] ([StudentId]);

CREATE INDEX [IX_Certificates_TrackId] ON [Certificates] ([TrackId]);

CREATE INDEX [IX_InteractiveActivities_TrackId] ON [InteractiveActivities] ([TrackId]);

CREATE INDEX [IX_InteractiveBookings_InteractiveActivityId] ON [InteractiveBookings] ([InteractiveActivityId]);

CREATE INDEX [IX_InteractiveBookings_UserId] ON [InteractiveBookings] ([UserId]);

CREATE INDEX [IX_LessonProgresses_LessonId] ON [LessonProgresses] ([LessonId]);

CREATE UNIQUE INDEX [IX_LessonProgresses_StudentId_LessonId] ON [LessonProgresses] ([StudentId], [LessonId]);

CREATE INDEX [IX_Lessons_TrackId] ON [Lessons] ([TrackId]);

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);

CREATE INDEX [IX_TrackEnrollments_StudentId] ON [TrackEnrollments] ([StudentId]);

CREATE INDEX [IX_TrackEnrollments_TrackId] ON [TrackEnrollments] ([TrackId]);

CREATE INDEX [IX_TrackInstructors_TrackId] ON [TrackInstructors] ([TrackId]);

CREATE INDEX [IX_TrackPackageBookings_TrackPackageId] ON [TrackPackageBookings] ([TrackPackageId]);

CREATE INDEX [IX_TrackPackageBookings_UserId] ON [TrackPackageBookings] ([UserId]);

CREATE INDEX [IX_TrackPackages_TrackId] ON [TrackPackages] ([TrackId]);

CREATE INDEX [IX_WorkshopBookings_UserId] ON [WorkshopBookings] ([UserId]);

CREATE INDEX [IX_WorkshopBookings_WorkshopId] ON [WorkshopBookings] ([WorkshopId]);

CREATE INDEX [IX_Workshops_TrackId] ON [Workshops] ([TrackId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260915124052_InitialLearningSystem', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Workshops] ADD [Price] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [Tracks] ADD [Price] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [InteractiveActivities] ADD [Price] decimal(18,2) NOT NULL DEFAULT 0.0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260916054151_AddPricesToBookableEntities', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [TrackPackageInteractives] (
    [TrackPackageId] int NOT NULL,
    [InteractiveActivityId] int NOT NULL,
    CONSTRAINT [PK_TrackPackageInteractives] PRIMARY KEY ([TrackPackageId], [InteractiveActivityId]),
    CONSTRAINT [FK_TrackPackageInteractives_InteractiveActivities_InteractiveActivityId] FOREIGN KEY ([InteractiveActivityId]) REFERENCES [InteractiveActivities] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TrackPackageInteractives_TrackPackages_TrackPackageId] FOREIGN KEY ([TrackPackageId]) REFERENCES [TrackPackages] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [TrackPackageWorkshops] (
    [TrackPackageId] int NOT NULL,
    [WorkshopId] int NOT NULL,
    CONSTRAINT [PK_TrackPackageWorkshops] PRIMARY KEY ([TrackPackageId], [WorkshopId]),
    CONSTRAINT [FK_TrackPackageWorkshops_TrackPackages_TrackPackageId] FOREIGN KEY ([TrackPackageId]) REFERENCES [TrackPackages] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TrackPackageWorkshops_Workshops_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshops] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_TrackPackageInteractives_InteractiveActivityId] ON [TrackPackageInteractives] ([InteractiveActivityId]);

CREATE INDEX [IX_TrackPackageWorkshops_WorkshopId] ON [TrackPackageWorkshops] ([WorkshopId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260917094222_AddTrackPackageWorkshop', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260920053012_FixTrackPackageRelationships', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
DROP INDEX [IX_TrackEnrollments_StudentId] ON [TrackEnrollments];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Lessons]') AND [c].[name] = N'ContentUrl');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Lessons] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Lessons] DROP COLUMN [ContentUrl];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Lessons]') AND [c].[name] = N'LessonType');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Lessons] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Lessons] DROP COLUMN [LessonType];

CREATE TABLE [LessonTypes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_LessonTypes] PRIMARY KEY ([Id])
);

CREATE TABLE [LessonContents] (
    [Id] int NOT NULL IDENTITY,
    [LessonId] int NOT NULL,
    [LessonTypeId] int NOT NULL,
    [ContentUrl] nvarchar(max) NULL,
    [OriginalFileName] nvarchar(max) NULL,
    [StoredFileName] nvarchar(max) NULL,
    [ContentType] nvarchar(max) NULL,
    [FileSize] bigint NULL,
    [IsFree] bit NULL,
    [WorkshopId] int NULL,
    [InteractiveActivityId] int NULL,
    CONSTRAINT [PK_LessonContents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LessonContents_InteractiveActivities_InteractiveActivityId] FOREIGN KEY ([InteractiveActivityId]) REFERENCES [InteractiveActivities] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LessonContents_LessonTypes_LessonTypeId] FOREIGN KEY ([LessonTypeId]) REFERENCES [LessonTypes] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LessonContents_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_LessonContents_Workshops_WorkshopId] FOREIGN KEY ([WorkshopId]) REFERENCES [Workshops] ([Id]) ON DELETE NO ACTION
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[LessonTypes]'))
    SET IDENTITY_INSERT [LessonTypes] ON;
INSERT INTO [LessonTypes] ([Id], [Name])
VALUES (1, N'Video'),
(2, N'PDF'),
(3, N'PowerPoint'),
(4, N'Interactive'),
(5, N'Workshop');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[LessonTypes]'))
    SET IDENTITY_INSERT [LessonTypes] OFF;

CREATE UNIQUE INDEX [IX_TrackEnrollments_StudentId_TrackId] ON [TrackEnrollments] ([StudentId], [TrackId]);

CREATE INDEX [IX_LessonContents_InteractiveActivityId] ON [LessonContents] ([InteractiveActivityId]);

CREATE INDEX [IX_LessonContents_LessonId] ON [LessonContents] ([LessonId]);

CREATE INDEX [IX_LessonContents_LessonTypeId] ON [LessonContents] ([LessonTypeId]);

CREATE INDEX [IX_LessonContents_WorkshopId] ON [LessonContents] ([WorkshopId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260926010259_AddLessonContentAndFinalizeDomain', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [WorkshopBookings] ADD [Source] int NOT NULL DEFAULT 0;

ALTER TABLE [InteractiveBookings] ADD [Source] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260930164520_AddBookingSource', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [TrackPackages] ADD [IncludesWorkshop] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260930194550_AddIncludesWorkshopToTrackPackage', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
DROP TABLE [TrackPackageBookings];

DROP TABLE [TrackPackageInteractives];

DROP TABLE [TrackPackageWorkshops];

DROP TABLE [TrackPackages];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260930223113_RemoveTrackPackageSystem', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [TrackEnrollments] ADD [BookingType] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261001022009_AddBookingTypeToTrackEnrollment', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [CoachingConversations] (
    [Id] int NOT NULL IDENTITY,
    [StudentId] nvarchar(450) NOT NULL,
    [TrackId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_CoachingConversations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CoachingConversations_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CoachingConversations_Tracks_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [Tracks] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CoachingMessages] (
    [Id] int NOT NULL IDENTITY,
    [ConversationId] int NOT NULL,
    [SenderId] nvarchar(450) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [SentAt] datetime2 NOT NULL,
    [IsRead] bit NOT NULL,
    CONSTRAINT [PK_CoachingMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CoachingMessages_AspNetUsers_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CoachingMessages_CoachingConversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [CoachingConversations] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_CoachingConversations_StudentId] ON [CoachingConversations] ([StudentId]);

CREATE INDEX [IX_CoachingConversations_TrackId] ON [CoachingConversations] ([TrackId]);

CREATE INDEX [IX_CoachingMessages_ConversationId] ON [CoachingMessages] ([ConversationId]);

CREATE INDEX [IX_CoachingMessages_SenderId] ON [CoachingMessages] ([SenderId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261001022851_AddCoaching', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Workshops] ADD [ApplicationUserId] nvarchar(450) NULL;

ALTER TABLE [Workshops] ADD [InstructorId] nvarchar(450) NULL DEFAULT N'';

ALTER TABLE [InteractiveActivities] ADD [ApplicationUserId] nvarchar(450) NULL;

ALTER TABLE [InteractiveActivities] ADD [InstructorId] nvarchar(450) NULL DEFAULT N'';

CREATE INDEX [IX_Workshops_ApplicationUserId] ON [Workshops] ([ApplicationUserId]);

CREATE INDEX [IX_Workshops_InstructorId] ON [Workshops] ([InstructorId]);

CREATE INDEX [IX_InteractiveActivities_ApplicationUserId] ON [InteractiveActivities] ([ApplicationUserId]);

CREATE INDEX [IX_InteractiveActivities_InstructorId] ON [InteractiveActivities] ([InstructorId]);

ALTER TABLE [InteractiveActivities] ADD CONSTRAINT [FK_InteractiveActivities_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [InteractiveActivities] ADD CONSTRAINT [FK_InteractiveActivities_AspNetUsers_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;

ALTER TABLE [Workshops] ADD CONSTRAINT [FK_Workshops_AspNetUsers_ApplicationUserId] FOREIGN KEY ([ApplicationUserId]) REFERENCES [AspNetUsers] ([Id]);

ALTER TABLE [Workshops] ADD CONSTRAINT [FK_Workshops_AspNetUsers_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261019182601_AddInstructorOwnershipToSessions', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Workshops]') AND [c].[name] = N'InstructorId');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Workshops] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Workshops] ALTER COLUMN [InstructorId] nvarchar(450) NULL;

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[InteractiveActivities]') AND [c].[name] = N'InstructorId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [InteractiveActivities] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [InteractiveActivities] ALTER COLUMN [InstructorId] nvarchar(450) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261019194238_fixnullableprblem', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [AssignmentSubmissions] ADD [ContentType] nvarchar(max) NULL;

ALTER TABLE [AssignmentSubmissions] ADD [FileSize] bigint NULL;

ALTER TABLE [AssignmentSubmissions] ADD [OriginalFileName] nvarchar(max) NULL;

ALTER TABLE [AssignmentSubmissions] ADD [StoredFileName] nvarchar(max) NULL;

ALTER TABLE [Assignments] ADD [AttachmentContentType] nvarchar(max) NULL;

ALTER TABLE [Assignments] ADD [AttachmentFileSize] bigint NULL;

ALTER TABLE [Assignments] ADD [AttachmentOriginalFileName] nvarchar(max) NULL;

ALTER TABLE [Assignments] ADD [AttachmentStoredFileName] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261020091232_AddAssignmentFileMetadata', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [WorkshopBookings] ADD [ApprovalStatus] int NOT NULL DEFAULT 0;

ALTER TABLE [TrackEnrollments] ADD [ApprovalStatus] int NOT NULL DEFAULT 0;

ALTER TABLE [TrackEnrollments] ADD [ApprovedPrice] decimal(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE [InteractiveBookings] ADD [ApprovalStatus] int NOT NULL DEFAULT 0;

CREATE TABLE [TrackBookingRequests] (
    [Id] int NOT NULL IDENTITY,
    [StudentId] nvarchar(450) NOT NULL,
    [TrackId] int NOT NULL,
    [BookingType] int NOT NULL,
    [ApprovalStatus] int NOT NULL,
    [RequestedPrice] decimal(18,2) NOT NULL,
    [RequestedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_TrackBookingRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TrackBookingRequests_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TrackBookingRequests_Tracks_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [Tracks] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_TrackBookingRequests_StudentId] ON [TrackBookingRequests] ([StudentId]);

CREATE INDEX [IX_TrackBookingRequests_TrackId] ON [TrackBookingRequests] ([TrackId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261025081803_AddBookingApprovalWorkflow', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [LessonQuizzes] (
    [Id] int NOT NULL IDENTITY,
    [LessonId] int NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Type] int NOT NULL,
    [QuestionCount] int NOT NULL,
    [PassingScore] decimal(5,2) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_LessonQuizzes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LessonQuizzes_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [LessonUnlocks] (
    [Id] int NOT NULL IDENTITY,
    [StudentId] nvarchar(450) NOT NULL,
    [LessonId] int NOT NULL,
    [UnlockedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_LessonUnlocks] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LessonUnlocks_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_LessonUnlocks_Lessons_LessonId] FOREIGN KEY ([LessonId]) REFERENCES [Lessons] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [QuizAttempts] (
    [Id] int NOT NULL IDENTITY,
    [LessonQuizId] int NOT NULL,
    [StudentId] nvarchar(450) NOT NULL,
    [StartedAt] datetime2 NOT NULL,
    [SubmittedAt] datetime2 NULL,
    [TotalQuestions] int NOT NULL,
    [CorrectAnswers] int NOT NULL,
    [Score] decimal(5,2) NOT NULL,
    [Passed] bit NOT NULL,
    CONSTRAINT [PK_QuizAttempts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QuizAttempts_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_QuizAttempts_LessonQuizzes_LessonQuizId] FOREIGN KEY ([LessonQuizId]) REFERENCES [LessonQuizzes] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [QuizQuestions] (
    [Id] int NOT NULL IDENTITY,
    [LessonQuizId] int NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    [Order] int NOT NULL,
    CONSTRAINT [PK_QuizQuestions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QuizQuestions_LessonQuizzes_LessonQuizId] FOREIGN KEY ([LessonQuizId]) REFERENCES [LessonQuizzes] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [QuizOptions] (
    [Id] int NOT NULL IDENTITY,
    [QuizQuestionId] int NOT NULL,
    [Text] nvarchar(max) NOT NULL,
    [IsCorrect] bit NOT NULL,
    CONSTRAINT [PK_QuizOptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QuizOptions_QuizQuestions_QuizQuestionId] FOREIGN KEY ([QuizQuestionId]) REFERENCES [QuizQuestions] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [QuizAnswers] (
    [Id] int NOT NULL IDENTITY,
    [QuizAttemptId] int NOT NULL,
    [QuizQuestionId] int NOT NULL,
    [SelectedOptionId] int NOT NULL,
    [IsCorrect] bit NOT NULL,
    CONSTRAINT [PK_QuizAnswers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_QuizAnswers_QuizAttempts_QuizAttemptId] FOREIGN KEY ([QuizAttemptId]) REFERENCES [QuizAttempts] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_QuizAnswers_QuizOptions_SelectedOptionId] FOREIGN KEY ([SelectedOptionId]) REFERENCES [QuizOptions] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_QuizAnswers_QuizQuestions_QuizQuestionId] FOREIGN KEY ([QuizQuestionId]) REFERENCES [QuizQuestions] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_LessonQuizzes_LessonId] ON [LessonQuizzes] ([LessonId]);

CREATE INDEX [IX_LessonUnlocks_LessonId] ON [LessonUnlocks] ([LessonId]);

CREATE UNIQUE INDEX [IX_LessonUnlocks_StudentId_LessonId] ON [LessonUnlocks] ([StudentId], [LessonId]);

CREATE UNIQUE INDEX [IX_QuizAnswers_QuizAttemptId_QuizQuestionId] ON [QuizAnswers] ([QuizAttemptId], [QuizQuestionId]);

CREATE INDEX [IX_QuizAnswers_QuizQuestionId] ON [QuizAnswers] ([QuizQuestionId]);

CREATE INDEX [IX_QuizAnswers_SelectedOptionId] ON [QuizAnswers] ([SelectedOptionId]);

CREATE INDEX [IX_QuizAttempts_LessonQuizId] ON [QuizAttempts] ([LessonQuizId]);

CREATE INDEX [IX_QuizAttempts_StudentId] ON [QuizAttempts] ([StudentId]);

CREATE INDEX [IX_QuizOptions_QuizQuestionId] ON [QuizOptions] ([QuizQuestionId]);

CREATE INDEX [IX_QuizQuestions_LessonQuizId] ON [QuizQuestions] ([LessonQuizId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261027165609_AddQuizSystem', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [InteractiveActivities] ADD [InteractiveScheduleId] int NULL;

CREATE TABLE [InteractiveSchedules] (
    [Id] int NOT NULL IDENTITY,
    [TrackId] int NOT NULL,
    [InstructorId] nvarchar(450) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Price] decimal(18,2) NOT NULL,
    [Capacity] int NOT NULL,
    [MeetingUrl] nvarchar(max) NULL,
    [DaysOfWeek] nvarchar(max) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_InteractiveSchedules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InteractiveSchedules_AspNetUsers_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InteractiveSchedules_Tracks_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [Tracks] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_InteractiveActivities_InteractiveScheduleId] ON [InteractiveActivities] ([InteractiveScheduleId]);

CREATE INDEX [IX_InteractiveSchedules_InstructorId] ON [InteractiveSchedules] ([InstructorId]);

CREATE INDEX [IX_InteractiveSchedules_TrackId] ON [InteractiveSchedules] ([TrackId]);

ALTER TABLE [InteractiveActivities] ADD CONSTRAINT [FK_InteractiveActivities_InteractiveSchedules_InteractiveScheduleId] FOREIGN KEY ([InteractiveScheduleId]) REFERENCES [InteractiveSchedules] ([Id]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261028165823_AddInteractiveSchedules', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [AspNetUsers] ADD [AccountType] int NOT NULL DEFAULT 0;

CREATE TABLE [PrivateArrangements] (
    [Id] int NOT NULL IDENTITY,
    [StudentId] nvarchar(450) NOT NULL,
    [InstructorId] nvarchar(450) NOT NULL,
    [DayOfWeek] int NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    [ApprovalStatus] int NOT NULL,
    [TrackId] int NOT NULL,
    CONSTRAINT [PK_PrivateArrangements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PrivateArrangements_AspNetUsers_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PrivateArrangements_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PrivateArrangements_Tracks_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [Tracks] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PrivateAvailabilities] (
    [Id] int NOT NULL IDENTITY,
    [InstructorId] nvarchar(450) NOT NULL,
    [DayOfWeek] int NOT NULL,
    [StartTime] time NOT NULL,
    [EndTime] time NOT NULL,
    [ValidFrom] datetime2 NOT NULL,
    [ValidTo] datetime2 NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_PrivateAvailabilities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PrivateAvailabilities_AspNetUsers_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PrivateOccurrences] (
    [Id] int NOT NULL IDENTITY,
    [PrivateArrangementId] int NULL,
    [StudentId] nvarchar(450) NOT NULL,
    [InstructorId] nvarchar(450) NOT NULL,
    [StartUtc] datetime2 NOT NULL,
    [EndUtc] datetime2 NOT NULL,
    [Status] int NOT NULL,
    [TrackId] int NOT NULL,
    CONSTRAINT [PK_PrivateOccurrences] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PrivateOccurrences_AspNetUsers_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PrivateOccurrences_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PrivateOccurrences_PrivateArrangements_PrivateArrangementId] FOREIGN KEY ([PrivateArrangementId]) REFERENCES [PrivateArrangements] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PrivateOccurrences_Tracks_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [Tracks] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PrivateReplacements] (
    [Id] int NOT NULL IDENTITY,
    [MissedOccurrenceId] int NOT NULL,
    [ReplacementOccurrenceId] int NOT NULL,
    [StudentId] nvarchar(max) NOT NULL,
    [AssignedByAdminId] nvarchar(max) NOT NULL,
    [AssignedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_PrivateReplacements] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PrivateReplacements_PrivateOccurrences_MissedOccurrenceId] FOREIGN KEY ([MissedOccurrenceId]) REFERENCES [PrivateOccurrences] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_PrivateReplacements_PrivateOccurrences_ReplacementOccurrenceId] FOREIGN KEY ([ReplacementOccurrenceId]) REFERENCES [PrivateOccurrences] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PrivateRequests] (
    [Id] int NOT NULL IDENTITY,
    [StudentId] nvarchar(max) NOT NULL,
    [OccurrenceId] int NOT NULL,
    [RequestType] int NOT NULL,
    [Status] int NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_PrivateRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PrivateRequests_PrivateOccurrences_OccurrenceId] FOREIGN KEY ([OccurrenceId]) REFERENCES [PrivateOccurrences] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_PrivateArrangements_InstructorId] ON [PrivateArrangements] ([InstructorId]);

CREATE INDEX [IX_PrivateArrangements_StudentId] ON [PrivateArrangements] ([StudentId]);

CREATE INDEX [IX_PrivateArrangements_TrackId] ON [PrivateArrangements] ([TrackId]);

CREATE INDEX [IX_PrivateAvailabilities_InstructorId_DayOfWeek] ON [PrivateAvailabilities] ([InstructorId], [DayOfWeek]);

CREATE INDEX [IX_PrivateOccurrences_InstructorId] ON [PrivateOccurrences] ([InstructorId]);

CREATE INDEX [IX_PrivateOccurrences_PrivateArrangementId] ON [PrivateOccurrences] ([PrivateArrangementId]);

CREATE INDEX [IX_PrivateOccurrences_StartUtc] ON [PrivateOccurrences] ([StartUtc]);

CREATE INDEX [IX_PrivateOccurrences_StudentId] ON [PrivateOccurrences] ([StudentId]);

CREATE INDEX [IX_PrivateOccurrences_TrackId] ON [PrivateOccurrences] ([TrackId]);

CREATE UNIQUE INDEX [IX_PrivateReplacements_MissedOccurrenceId] ON [PrivateReplacements] ([MissedOccurrenceId]);

CREATE INDEX [IX_PrivateReplacements_ReplacementOccurrenceId] ON [PrivateReplacements] ([ReplacementOccurrenceId]);

CREATE INDEX [IX_PrivateRequests_OccurrenceId] ON [PrivateRequests] ([OccurrenceId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261109213708_AddPrivateSessions', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
DROP INDEX [IX_PrivateRequests_OccurrenceId] ON [PrivateRequests];

DROP INDEX [IX_PrivateReplacements_ReplacementOccurrenceId] ON [PrivateReplacements];

DROP INDEX [IX_PrivateOccurrences_PrivateArrangementId] ON [PrivateOccurrences];

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PrivateRequests]') AND [c].[name] = N'StudentId');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [PrivateRequests] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [PrivateRequests] ALTER COLUMN [StudentId] nvarchar(450) NOT NULL;

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PrivateReplacements]') AND [c].[name] = N'StudentId');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [PrivateReplacements] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [PrivateReplacements] ALTER COLUMN [StudentId] nvarchar(450) NOT NULL;

DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PrivateReplacements]') AND [c].[name] = N'AssignedByAdminId');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [PrivateReplacements] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [PrivateReplacements] ALTER COLUMN [AssignedByAdminId] nvarchar(450) NOT NULL;

CREATE UNIQUE INDEX [IX_PrivateRequests_OccurrenceId_RequestType] ON [PrivateRequests] ([OccurrenceId], [RequestType]) WHERE [Status] = 0;

CREATE INDEX [IX_PrivateRequests_OccurrenceId_RequestType_Status] ON [PrivateRequests] ([OccurrenceId], [RequestType], [Status]);

CREATE INDEX [IX_PrivateRequests_StudentId] ON [PrivateRequests] ([StudentId]);

CREATE INDEX [IX_PrivateReplacements_AssignedByAdminId] ON [PrivateReplacements] ([AssignedByAdminId]);

CREATE UNIQUE INDEX [IX_PrivateReplacements_ReplacementOccurrenceId] ON [PrivateReplacements] ([ReplacementOccurrenceId]);

CREATE INDEX [IX_PrivateReplacements_StudentId] ON [PrivateReplacements] ([StudentId]);

CREATE UNIQUE INDEX [IX_PrivateOccurrences_PrivateArrangementId_StartUtc] ON [PrivateOccurrences] ([PrivateArrangementId], [StartUtc]) WHERE [PrivateArrangementId] IS NOT NULL;

ALTER TABLE [PrivateReplacements] ADD CONSTRAINT [FK_PrivateReplacements_AspNetUsers_AssignedByAdminId] FOREIGN KEY ([AssignedByAdminId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [PrivateReplacements] ADD CONSTRAINT [FK_PrivateReplacements_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

ALTER TABLE [PrivateRequests] ADD CONSTRAINT [FK_PrivateRequests_AspNetUsers_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261111001650_CheckPrivateSessionsFinalChanges', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [SupportChatConversations] (
    [Id] int NOT NULL IDENTITY,
    [Type] int NOT NULL,
    [ParticipantId] nvarchar(450) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_SupportChatConversations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupportChatConversations_AspNetUsers_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [SupportChatMessages] (
    [Id] int NOT NULL IDENTITY,
    [ConversationId] int NOT NULL,
    [SenderId] nvarchar(450) NOT NULL,
    [Message] nvarchar(max) NOT NULL,
    [SentAtUtc] datetime2 NOT NULL,
    [IsRead] bit NOT NULL,
    CONSTRAINT [PK_SupportChatMessages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SupportChatMessages_AspNetUsers_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SupportChatMessages_SupportChatConversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [SupportChatConversations] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_SupportChatConversations_CreatedAtUtc] ON [SupportChatConversations] ([CreatedAtUtc]);

CREATE INDEX [IX_SupportChatConversations_ParticipantId] ON [SupportChatConversations] ([ParticipantId]);

CREATE UNIQUE INDEX [IX_SupportChatConversations_Type_ParticipantId] ON [SupportChatConversations] ([Type], [ParticipantId]);

CREATE INDEX [IX_SupportChatMessages_ConversationId_IsRead] ON [SupportChatMessages] ([ConversationId], [IsRead]);

CREATE INDEX [IX_SupportChatMessages_ConversationId_SentAtUtc] ON [SupportChatMessages] ([ConversationId], [SentAtUtc]);

CREATE INDEX [IX_SupportChatMessages_SenderId] ON [SupportChatMessages] ([SenderId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261111183718_AddSupportChatSystem', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [PrivateOccurrences] ADD [MeetingLink] nvarchar(max) NULL;

ALTER TABLE [PrivateArrangements] ADD [MeetingLink] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261112223133_AddMeetingLinksToPrivateSessions', N'10.0.12');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Workshops] ADD [ApprovalStatus] int NOT NULL DEFAULT 0;

ALTER TABLE [InteractiveActivities] ADD [ApprovalStatus] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261118233656_AddActivityApprovalStatus', N'10.0.12');

COMMIT;
GO

