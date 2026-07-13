using FluentMigrator;

namespace Whitebird.Migrations.Features.AssetTransaction
{
    [Migration(20260713205218)]
    public class CreateTableAssetTransactionAttachment : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AssetTransactionAttachment')
        BEGIN
            CREATE TABLE [dbo].[AssetTransactionAttachment] (
                [AssetTransactionAttachmentId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_AssetTransactionAttachment_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_AssetTransactionAttachment_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_AssetTransactionAttachment_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [AssetTransactionId] INT NOT NULL,
                [FileAttachmentId] INT NOT NULL,
                [Order] INT NOT NULL CONSTRAINT [DF_AssetTransactionAttachment_Order] DEFAULT 0,
                CONSTRAINT [PK_AssetTransactionAttachment] PRIMARY KEY ([AssetTransactionAttachmentId]),
                CONSTRAINT [FK_AssetTransactionAttachment_AssetTransaction]
                    FOREIGN KEY ([AssetTransactionId]) REFERENCES [dbo].[AssetTransaction]([AssetTransactionId]),
                CONSTRAINT [FK_AssetTransactionAttachment_FileAttachment]
                    FOREIGN KEY ([FileAttachmentId]) REFERENCES [dbo].[FileAttachment]([FileAttachmentId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionAttachment_AssetTransactionId' AND object_id = OBJECT_ID('AssetTransactionAttachment'))
            CREATE INDEX [IX_AssetTransactionAttachment_AssetTransactionId]
            ON [dbo].[AssetTransactionAttachment]([AssetTransactionId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionAttachment_FileAttachmentId' AND object_id = OBJECT_ID('AssetTransactionAttachment'))
            CREATE INDEX [IX_AssetTransactionAttachment_FileAttachmentId]
            ON [dbo].[AssetTransactionAttachment]([FileAttachmentId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionAttachment_AssetTransactionId_Order' AND object_id = OBJECT_ID('AssetTransactionAttachment'))
            CREATE INDEX [IX_AssetTransactionAttachment_AssetTransactionId_Order]
            ON [dbo].[AssetTransactionAttachment]([AssetTransactionId], [Order]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionAttachment_IsActive' AND object_id = OBJECT_ID('AssetTransactionAttachment'))
            CREATE INDEX [IX_AssetTransactionAttachment_IsActive]
            ON [dbo].[AssetTransactionAttachment]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionAttachment_IsActive' AND object_id = OBJECT_ID('AssetTransactionAttachment'))
            DROP INDEX [IX_AssetTransactionAttachment_IsActive] ON [dbo].[AssetTransactionAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionAttachment_AssetTransactionId_Order' AND object_id = OBJECT_ID('AssetTransactionAttachment'))
            DROP INDEX [IX_AssetTransactionAttachment_AssetTransactionId_Order] ON [dbo].[AssetTransactionAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionAttachment_FileAttachmentId' AND object_id = OBJECT_ID('AssetTransactionAttachment'))
            DROP INDEX [IX_AssetTransactionAttachment_FileAttachmentId] ON [dbo].[AssetTransactionAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionAttachment_AssetTransactionId' AND object_id = OBJECT_ID('AssetTransactionAttachment'))
            DROP INDEX [IX_AssetTransactionAttachment_AssetTransactionId] ON [dbo].[AssetTransactionAttachment];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AssetTransactionAttachment')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AssetTransactionAttachment') AND name = 'FK_AssetTransactionAttachment_FileAttachment')
                ALTER TABLE [dbo].[AssetTransactionAttachment] DROP CONSTRAINT [FK_AssetTransactionAttachment_FileAttachment];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AssetTransactionAttachment') AND name = 'FK_AssetTransactionAttachment_AssetTransaction')
                ALTER TABLE [dbo].[AssetTransactionAttachment] DROP CONSTRAINT [FK_AssetTransactionAttachment_AssetTransaction];
            
            DROP TABLE [dbo].[AssetTransactionAttachment];
        END
    ");
        }
    }
}