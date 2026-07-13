using FluentMigrator;

namespace Whitebird.Migrations.Features.Asset
{
    [Migration(20260713205119)]
    public class CreateTableAssetPhotoProfileAttachment : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AssetPhotoProfileAttachment')
        BEGIN
            CREATE TABLE [dbo].[AssetPhotoProfileAttachment] (
                [AssetPhotoProfileAttachmentId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_AssetPhotoProfileAttachment_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_AssetPhotoProfileAttachment_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_AssetPhotoProfileAttachment_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [AssetId] INT NOT NULL,
                [FileAttachmentId] INT NOT NULL,
                CONSTRAINT [PK_AssetPhotoProfileAttachment] PRIMARY KEY ([AssetPhotoProfileAttachmentId]),
                CONSTRAINT [FK_AssetPhotoProfileAttachment_Asset]
                    FOREIGN KEY ([AssetId]) REFERENCES [dbo].[Asset]([AssetId]),
                CONSTRAINT [FK_AssetPhotoProfileAttachment_FileAttachment]
                    FOREIGN KEY ([FileAttachmentId]) REFERENCES [dbo].[FileAttachment]([FileAttachmentId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetPhotoProfileAttachment_AssetId' AND object_id = OBJECT_ID('AssetPhotoProfileAttachment'))
            CREATE INDEX [IX_AssetPhotoProfileAttachment_AssetId]
            ON [dbo].[AssetPhotoProfileAttachment]([AssetId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetPhotoProfileAttachment_FileAttachmentId' AND object_id = OBJECT_ID('AssetPhotoProfileAttachment'))
            CREATE INDEX [IX_AssetPhotoProfileAttachment_FileAttachmentId]
            ON [dbo].[AssetPhotoProfileAttachment]([FileAttachmentId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetPhotoProfileAttachment_AssetId_IsActive' AND object_id = OBJECT_ID('AssetPhotoProfileAttachment'))
            CREATE INDEX [IX_AssetPhotoProfileAttachment_AssetId_IsActive]
            ON [dbo].[AssetPhotoProfileAttachment]([AssetId], [IsActive])
            WHERE [IsActive] = 1;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetPhotoProfileAttachment_IsActive' AND object_id = OBJECT_ID('AssetPhotoProfileAttachment'))
            CREATE INDEX [IX_AssetPhotoProfileAttachment_IsActive]
            ON [dbo].[AssetPhotoProfileAttachment]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetPhotoProfileAttachment_IsActive' AND object_id = OBJECT_ID('AssetPhotoProfileAttachment'))
            DROP INDEX [IX_AssetPhotoProfileAttachment_IsActive] ON [dbo].[AssetPhotoProfileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetPhotoProfileAttachment_AssetId_IsActive' AND object_id = OBJECT_ID('AssetPhotoProfileAttachment'))
            DROP INDEX [IX_AssetPhotoProfileAttachment_AssetId_IsActive] ON [dbo].[AssetPhotoProfileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetPhotoProfileAttachment_FileAttachmentId' AND object_id = OBJECT_ID('AssetPhotoProfileAttachment'))
            DROP INDEX [IX_AssetPhotoProfileAttachment_FileAttachmentId] ON [dbo].[AssetPhotoProfileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetPhotoProfileAttachment_AssetId' AND object_id = OBJECT_ID('AssetPhotoProfileAttachment'))
            DROP INDEX [IX_AssetPhotoProfileAttachment_AssetId] ON [dbo].[AssetPhotoProfileAttachment];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AssetPhotoProfileAttachment')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AssetPhotoProfileAttachment') AND name = 'FK_AssetPhotoProfileAttachment_FileAttachment')
                ALTER TABLE [dbo].[AssetPhotoProfileAttachment] DROP CONSTRAINT [FK_AssetPhotoProfileAttachment_FileAttachment];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AssetPhotoProfileAttachment') AND name = 'FK_AssetPhotoProfileAttachment_Asset')
                ALTER TABLE [dbo].[AssetPhotoProfileAttachment] DROP CONSTRAINT [FK_AssetPhotoProfileAttachment_Asset];
            
            DROP TABLE [dbo].[AssetPhotoProfileAttachment];
        END
    ");
        }
    }
}