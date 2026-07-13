using FluentMigrator;

namespace Whitebird.Migrations.Features.User
{
    [Migration(20260713205504)]
    public class CreateTableUsersPhotoProfileAttachment : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UsersPhotoProfileAttachment')
        BEGIN
            CREATE TABLE [dbo].[UsersPhotoProfileAttachment] (
                [UsersPhotoProfileAttachmentId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_UsersPhotoProfileAttachment_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_UsersPhotoProfileAttachment_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_UsersPhotoProfileAttachment_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [UserId] INT NOT NULL,
                [FileAttachmentId] INT NOT NULL,
                CONSTRAINT [PK_UsersPhotoProfileAttachment] PRIMARY KEY ([UsersPhotoProfileAttachmentId]),
                CONSTRAINT [FK_UsersPhotoProfileAttachment_Users]
                    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]),
                CONSTRAINT [FK_UsersPhotoProfileAttachment_FileAttachment]
                    FOREIGN KEY ([FileAttachmentId]) REFERENCES [dbo].[FileAttachment]([FileAttachmentId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UsersPhotoProfileAttachment_UserId' AND object_id = OBJECT_ID('UsersPhotoProfileAttachment'))
            CREATE INDEX [IX_UsersPhotoProfileAttachment_UserId]
            ON [dbo].[UsersPhotoProfileAttachment]([UserId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UsersPhotoProfileAttachment_FileAttachmentId' AND object_id = OBJECT_ID('UsersPhotoProfileAttachment'))
            CREATE INDEX [IX_UsersPhotoProfileAttachment_FileAttachmentId]
            ON [dbo].[UsersPhotoProfileAttachment]([FileAttachmentId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UsersPhotoProfileAttachment_UserId_IsActive' AND object_id = OBJECT_ID('UsersPhotoProfileAttachment'))
            CREATE INDEX [IX_UsersPhotoProfileAttachment_UserId_IsActive]
            ON [dbo].[UsersPhotoProfileAttachment]([UserId], [IsActive])
            WHERE [IsActive] = 1;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UsersPhotoProfileAttachment_IsActive' AND object_id = OBJECT_ID('UsersPhotoProfileAttachment'))
            CREATE INDEX [IX_UsersPhotoProfileAttachment_IsActive]
            ON [dbo].[UsersPhotoProfileAttachment]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UsersPhotoProfileAttachment_IsActive' AND object_id = OBJECT_ID('UsersPhotoProfileAttachment'))
            DROP INDEX [IX_UsersPhotoProfileAttachment_IsActive] ON [dbo].[UsersPhotoProfileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UsersPhotoProfileAttachment_UserId_IsActive' AND object_id = OBJECT_ID('UsersPhotoProfileAttachment'))
            DROP INDEX [IX_UsersPhotoProfileAttachment_UserId_IsActive] ON [dbo].[UsersPhotoProfileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UsersPhotoProfileAttachment_FileAttachmentId' AND object_id = OBJECT_ID('UsersPhotoProfileAttachment'))
            DROP INDEX [IX_UsersPhotoProfileAttachment_FileAttachmentId] ON [dbo].[UsersPhotoProfileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UsersPhotoProfileAttachment_UserId' AND object_id = OBJECT_ID('UsersPhotoProfileAttachment'))
            DROP INDEX [IX_UsersPhotoProfileAttachment_UserId] ON [dbo].[UsersPhotoProfileAttachment];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UsersPhotoProfileAttachment')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('UsersPhotoProfileAttachment') AND name = 'FK_UsersPhotoProfileAttachment_FileAttachment')
                ALTER TABLE [dbo].[UsersPhotoProfileAttachment] DROP CONSTRAINT [FK_UsersPhotoProfileAttachment_FileAttachment];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('UsersPhotoProfileAttachment') AND name = 'FK_UsersPhotoProfileAttachment_Users')
                ALTER TABLE [dbo].[UsersPhotoProfileAttachment] DROP CONSTRAINT [FK_UsersPhotoProfileAttachment_Users];
            
            DROP TABLE [dbo].[UsersPhotoProfileAttachment];
        END
    ");
        }
    }
}