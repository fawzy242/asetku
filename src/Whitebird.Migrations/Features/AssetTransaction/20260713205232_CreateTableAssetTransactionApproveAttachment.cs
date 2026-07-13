using FluentMigrator;

namespace Whitebird.Migrations.Features.AssetTransaction
{
    [Migration(20260713205232)]
    public class CreateTableAssetTransactionApproveAttachment : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AssetTransactionApproveAttachment')
        BEGIN
            CREATE TABLE [dbo].[AssetTransactionApproveAttachment] (
                [AssetTransactionApproveAttachmentId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_AssetTransactionApproveAttachment_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_AssetTransactionApproveAttachment_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_AssetTransactionApproveAttachment_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [AssetTransactionId] INT NOT NULL,
                [FileAttachmentId] INT NOT NULL,
                [Order] INT NOT NULL CONSTRAINT [DF_AssetTransactionApproveAttachment_Order] DEFAULT 0,
                [DocumentType] INT NOT NULL,
                [NeedToChange] BIT NOT NULL CONSTRAINT [DF_AssetTransactionApproveAttachment_NeedToChange] DEFAULT 0,
                [Approved] BIT NOT NULL CONSTRAINT [DF_AssetTransactionApproveAttachment_Approved] DEFAULT 0,
                CONSTRAINT [PK_AssetTransactionApproveAttachment] PRIMARY KEY ([AssetTransactionApproveAttachmentId]),
                CONSTRAINT [FK_AssetTransactionApproveAttachment_AssetTransaction]
                    FOREIGN KEY ([AssetTransactionId]) REFERENCES [dbo].[AssetTransaction]([AssetTransactionId]),
                CONSTRAINT [FK_AssetTransactionApproveAttachment_FileAttachment]
                    FOREIGN KEY ([FileAttachmentId]) REFERENCES [dbo].[FileAttachment]([FileAttachmentId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_AssetTransactionId' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            CREATE INDEX [IX_AssetTransactionApproveAttachment_AssetTransactionId]
            ON [dbo].[AssetTransactionApproveAttachment]([AssetTransactionId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_FileAttachmentId' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            CREATE INDEX [IX_AssetTransactionApproveAttachment_FileAttachmentId]
            ON [dbo].[AssetTransactionApproveAttachment]([FileAttachmentId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_AssetTransactionId_Order' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            CREATE INDEX [IX_AssetTransactionApproveAttachment_AssetTransactionId_Order]
            ON [dbo].[AssetTransactionApproveAttachment]([AssetTransactionId], [Order]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_DocumentType' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            CREATE INDEX [IX_AssetTransactionApproveAttachment_DocumentType]
            ON [dbo].[AssetTransactionApproveAttachment]([AssetTransactionId], [DocumentType]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_Approved' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            CREATE INDEX [IX_AssetTransactionApproveAttachment_Approved]
            ON [dbo].[AssetTransactionApproveAttachment]([AssetTransactionId], [Approved])
            WHERE [Approved] = 0;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_NeedToChange' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            CREATE INDEX [IX_AssetTransactionApproveAttachment_NeedToChange]
            ON [dbo].[AssetTransactionApproveAttachment]([AssetTransactionId], [NeedToChange])
            WHERE [NeedToChange] = 1;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_IsActive' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            CREATE INDEX [IX_AssetTransactionApproveAttachment_IsActive]
            ON [dbo].[AssetTransactionApproveAttachment]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_IsActive' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            DROP INDEX [IX_AssetTransactionApproveAttachment_IsActive] ON [dbo].[AssetTransactionApproveAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_NeedToChange' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            DROP INDEX [IX_AssetTransactionApproveAttachment_NeedToChange] ON [dbo].[AssetTransactionApproveAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_Approved' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            DROP INDEX [IX_AssetTransactionApproveAttachment_Approved] ON [dbo].[AssetTransactionApproveAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_DocumentType' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            DROP INDEX [IX_AssetTransactionApproveAttachment_DocumentType] ON [dbo].[AssetTransactionApproveAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_AssetTransactionId_Order' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            DROP INDEX [IX_AssetTransactionApproveAttachment_AssetTransactionId_Order] ON [dbo].[AssetTransactionApproveAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_FileAttachmentId' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            DROP INDEX [IX_AssetTransactionApproveAttachment_FileAttachmentId] ON [dbo].[AssetTransactionApproveAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransactionApproveAttachment_AssetTransactionId' AND object_id = OBJECT_ID('AssetTransactionApproveAttachment'))
            DROP INDEX [IX_AssetTransactionApproveAttachment_AssetTransactionId] ON [dbo].[AssetTransactionApproveAttachment];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AssetTransactionApproveAttachment')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AssetTransactionApproveAttachment') AND name = 'FK_AssetTransactionApproveAttachment_FileAttachment')
                ALTER TABLE [dbo].[AssetTransactionApproveAttachment] DROP CONSTRAINT [FK_AssetTransactionApproveAttachment_FileAttachment];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AssetTransactionApproveAttachment') AND name = 'FK_AssetTransactionApproveAttachment_AssetTransaction')
                ALTER TABLE [dbo].[AssetTransactionApproveAttachment] DROP CONSTRAINT [FK_AssetTransactionApproveAttachment_AssetTransaction];
            
            DROP TABLE [dbo].[AssetTransactionApproveAttachment];
        END
    ");
        }
    }
}