using FluentMigrator;

namespace Whitebird.Migrations.Features.FileAttachment
{
    [Migration(20260519213310)]
    public class CreateTableFileAttachment : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        IF NOT EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'FileAttachment' AND [xtype] = 'U')
        BEGIN
            CREATE TABLE [dbo].[FileAttachment] (
                [FileAttachmentId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_FileAttachment_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_FileAttachment_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_FileAttachment_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [ReferenceTable] NVARCHAR(100) NOT NULL,
                [ReferenceId] INT NOT NULL,
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
        END;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_FileAttachment_ReferenceTable_ReferenceId')
            CREATE INDEX [IX_FileAttachment_ReferenceTable_ReferenceId]
            ON [dbo].[FileAttachment]([ReferenceTable], [ReferenceId]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_FileAttachment_FileHash')
            CREATE INDEX [IX_FileAttachment_FileHash]
            ON [dbo].[FileAttachment]([FileHash]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_FileAttachment_CreatedDate')
            CREATE INDEX [IX_FileAttachment_CreatedDate]
            ON [dbo].[FileAttachment]([CreatedDate] DESC);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_FileAttachment_FileCategory')
            CREATE INDEX [IX_FileAttachment_FileCategory]
            ON [dbo].[FileAttachment]([FileCategory]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_FileAttachment_FileExtension')
            CREATE INDEX [IX_FileAttachment_FileExtension]
            ON [dbo].[FileAttachment]([FileExtension]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_FileAttachment_IsPrimary')
            CREATE INDEX [IX_FileAttachment_IsPrimary]
            ON [dbo].[FileAttachment]([ReferenceTable], [ReferenceId], [IsPrimary])
            WHERE [IsPrimary] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_FileAttachment_IsPrimary')
            DROP INDEX [IX_FileAttachment_IsPrimary] ON [dbo].[FileAttachment];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_FileAttachment_FileExtension')
            DROP INDEX [IX_FileAttachment_FileExtension] ON [dbo].[FileAttachment];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_FileAttachment_FileCategory')
            DROP INDEX [IX_FileAttachment_FileCategory] ON [dbo].[FileAttachment];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_FileAttachment_CreatedDate')
            DROP INDEX [IX_FileAttachment_CreatedDate] ON [dbo].[FileAttachment];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_FileAttachment_FileHash')
            DROP INDEX [IX_FileAttachment_FileHash] ON [dbo].[FileAttachment];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_FileAttachment_ReferenceTable_ReferenceId')
            DROP INDEX [IX_FileAttachment_ReferenceTable_ReferenceId] ON [dbo].[FileAttachment];

        IF EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'FileAttachment' AND [xtype] = 'U')
            DROP TABLE [dbo].[FileAttachment];
    ");
        }
    }
}
