using FluentMigrator;

namespace Whitebird.Migrations.Features.FileAttachment
{
    [Migration(20260519213310)]
    public class CreateTableFileAttachment : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FileAttachment')
        BEGIN
            CREATE TABLE [dbo].[FileAttachment] (
                [FileAttachmentId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_FileAttachment_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_FileAttachment_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_FileAttachment_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [FileCategory] NVARCHAR(50) NULL,
                [FileName] NVARCHAR(255) NOT NULL,
                [OriginalFileName] NVARCHAR(255) NOT NULL,
                [FileExtension] NVARCHAR(20) NULL,
                [FileMimeType] NVARCHAR(100) NULL,
                [FilePath] NVARCHAR(1000) NOT NULL,
                [FileSize] BIGINT NULL,
                [FileHash] NVARCHAR(255) NULL,
                [Description] NVARCHAR(500) NULL,
                [VersionNumber] INT NOT NULL CONSTRAINT [DF_FileAttachment_VersionNumber] DEFAULT 1,
                [IsPrimary] BIT NOT NULL CONSTRAINT [DF_FileAttachment_IsPrimary] DEFAULT 0,
                CONSTRAINT [PK_FileAttachment] PRIMARY KEY ([FileAttachmentId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileAttachment_FileHash' AND object_id = OBJECT_ID('FileAttachment'))
            CREATE INDEX [IX_FileAttachment_FileHash]
            ON [dbo].[FileAttachment]([FileHash])
            WHERE [FileHash] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileAttachment_CreatedDate' AND object_id = OBJECT_ID('FileAttachment'))
            CREATE INDEX [IX_FileAttachment_CreatedDate]
            ON [dbo].[FileAttachment]([CreatedDate] DESC);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileAttachment_FileCategory' AND object_id = OBJECT_ID('FileAttachment'))
            CREATE INDEX [IX_FileAttachment_FileCategory]
            ON [dbo].[FileAttachment]([FileCategory])
            WHERE [FileCategory] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileAttachment_FileExtension' AND object_id = OBJECT_ID('FileAttachment'))
            CREATE INDEX [IX_FileAttachment_FileExtension]
            ON [dbo].[FileAttachment]([FileExtension])
            WHERE [FileExtension] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileAttachment_FileName' AND object_id = OBJECT_ID('FileAttachment'))
            CREATE INDEX [IX_FileAttachment_FileName]
            ON [dbo].[FileAttachment]([FileName]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileAttachment_IsActive' AND object_id = OBJECT_ID('FileAttachment'))
            CREATE INDEX [IX_FileAttachment_IsActive]
            ON [dbo].[FileAttachment]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileAttachment_IsActive' AND object_id = OBJECT_ID('FileAttachment'))
            DROP INDEX [IX_FileAttachment_IsActive] ON [dbo].[FileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileAttachment_FileName' AND object_id = OBJECT_ID('FileAttachment'))
            DROP INDEX [IX_FileAttachment_FileName] ON [dbo].[FileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileAttachment_FileExtension' AND object_id = OBJECT_ID('FileAttachment'))
            DROP INDEX [IX_FileAttachment_FileExtension] ON [dbo].[FileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileAttachment_FileCategory' AND object_id = OBJECT_ID('FileAttachment'))
            DROP INDEX [IX_FileAttachment_FileCategory] ON [dbo].[FileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileAttachment_CreatedDate' AND object_id = OBJECT_ID('FileAttachment'))
            DROP INDEX [IX_FileAttachment_CreatedDate] ON [dbo].[FileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_FileAttachment_FileHash' AND object_id = OBJECT_ID('FileAttachment'))
            DROP INDEX [IX_FileAttachment_FileHash] ON [dbo].[FileAttachment];

        -- ============================================================
        -- DROP TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'FileAttachment')
            DROP TABLE [dbo].[FileAttachment];
    ");
        }
    }
}