using FluentMigrator;

namespace Whitebird.Migrations.Features.Asset
{
    [Migration(20260713205106)]
    public class CreateTableAssetAttachment : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AssetAttachment')
        BEGIN
            CREATE TABLE [dbo].[AssetAttachment] (
                [AssetAttachmentId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_AssetAttachment_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_AssetAttachment_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_AssetAttachment_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [AssetId] INT NOT NULL,
                [FileAttachmentId] INT NOT NULL,
                [Order] INT NOT NULL CONSTRAINT [DF_AssetAttachment_Order] DEFAULT 0,
                CONSTRAINT [PK_AssetAttachment] PRIMARY KEY ([AssetAttachmentId]),
                CONSTRAINT [FK_AssetAttachment_Asset]
                    FOREIGN KEY ([AssetId]) REFERENCES [dbo].[Asset]([AssetId]),
                CONSTRAINT [FK_AssetAttachment_FileAttachment]
                    FOREIGN KEY ([FileAttachmentId]) REFERENCES [dbo].[FileAttachment]([FileAttachmentId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetAttachment_AssetId' AND object_id = OBJECT_ID('AssetAttachment'))
            CREATE INDEX [IX_AssetAttachment_AssetId]
            ON [dbo].[AssetAttachment]([AssetId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetAttachment_FileAttachmentId' AND object_id = OBJECT_ID('AssetAttachment'))
            CREATE INDEX [IX_AssetAttachment_FileAttachmentId]
            ON [dbo].[AssetAttachment]([FileAttachmentId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetAttachment_AssetId_Order' AND object_id = OBJECT_ID('AssetAttachment'))
            CREATE INDEX [IX_AssetAttachment_AssetId_Order]
            ON [dbo].[AssetAttachment]([AssetId], [Order]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetAttachment_IsActive' AND object_id = OBJECT_ID('AssetAttachment'))
            CREATE INDEX [IX_AssetAttachment_IsActive]
            ON [dbo].[AssetAttachment]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetAttachment_IsActive' AND object_id = OBJECT_ID('AssetAttachment'))
            DROP INDEX [IX_AssetAttachment_IsActive] ON [dbo].[AssetAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetAttachment_AssetId_Order' AND object_id = OBJECT_ID('AssetAttachment'))
            DROP INDEX [IX_AssetAttachment_AssetId_Order] ON [dbo].[AssetAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetAttachment_FileAttachmentId' AND object_id = OBJECT_ID('AssetAttachment'))
            DROP INDEX [IX_AssetAttachment_FileAttachmentId] ON [dbo].[AssetAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetAttachment_AssetId' AND object_id = OBJECT_ID('AssetAttachment'))
            DROP INDEX [IX_AssetAttachment_AssetId] ON [dbo].[AssetAttachment];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AssetAttachment')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AssetAttachment') AND name = 'FK_AssetAttachment_FileAttachment')
                ALTER TABLE [dbo].[AssetAttachment] DROP CONSTRAINT [FK_AssetAttachment_FileAttachment];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AssetAttachment') AND name = 'FK_AssetAttachment_Asset')
                ALTER TABLE [dbo].[AssetAttachment] DROP CONSTRAINT [FK_AssetAttachment_Asset];
            
            DROP TABLE [dbo].[AssetAttachment];
        END
    ");
        }
    }
}