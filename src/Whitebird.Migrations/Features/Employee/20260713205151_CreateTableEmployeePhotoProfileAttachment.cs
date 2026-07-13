using FluentMigrator;

namespace Whitebird.Migrations.Features.Employee
{
    [Migration(20260713205151)]
    public class CreateTableEmployeePhotoProfileAttachment : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EmployeePhotoProfileAttachment')
        BEGIN
            CREATE TABLE [dbo].[EmployeePhotoProfileAttachment] (
                [EmployeePhotoProfileAttachmentId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_EmployeePhotoProfileAttachment_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_EmployeePhotoProfileAttachment_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_EmployeePhotoProfileAttachment_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [EmployeeId] INT NOT NULL,
                [FileAttachmentId] INT NOT NULL,
                CONSTRAINT [PK_EmployeePhotoProfileAttachment] PRIMARY KEY ([EmployeePhotoProfileAttachmentId]),
                CONSTRAINT [FK_EmployeePhotoProfileAttachment_Employee]
                    FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employee]([EmployeeId]),
                CONSTRAINT [FK_EmployeePhotoProfileAttachment_FileAttachment]
                    FOREIGN KEY ([FileAttachmentId]) REFERENCES [dbo].[FileAttachment]([FileAttachmentId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeePhotoProfileAttachment_EmployeeId' AND object_id = OBJECT_ID('EmployeePhotoProfileAttachment'))
            CREATE INDEX [IX_EmployeePhotoProfileAttachment_EmployeeId]
            ON [dbo].[EmployeePhotoProfileAttachment]([EmployeeId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeePhotoProfileAttachment_FileAttachmentId' AND object_id = OBJECT_ID('EmployeePhotoProfileAttachment'))
            CREATE INDEX [IX_EmployeePhotoProfileAttachment_FileAttachmentId]
            ON [dbo].[EmployeePhotoProfileAttachment]([FileAttachmentId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeePhotoProfileAttachment_EmployeeId_IsActive' AND object_id = OBJECT_ID('EmployeePhotoProfileAttachment'))
            CREATE INDEX [IX_EmployeePhotoProfileAttachment_EmployeeId_IsActive]
            ON [dbo].[EmployeePhotoProfileAttachment]([EmployeeId], [IsActive])
            WHERE [IsActive] = 1;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeePhotoProfileAttachment_IsActive' AND object_id = OBJECT_ID('EmployeePhotoProfileAttachment'))
            CREATE INDEX [IX_EmployeePhotoProfileAttachment_IsActive]
            ON [dbo].[EmployeePhotoProfileAttachment]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeePhotoProfileAttachment_IsActive' AND object_id = OBJECT_ID('EmployeePhotoProfileAttachment'))
            DROP INDEX [IX_EmployeePhotoProfileAttachment_IsActive] ON [dbo].[EmployeePhotoProfileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeePhotoProfileAttachment_EmployeeId_IsActive' AND object_id = OBJECT_ID('EmployeePhotoProfileAttachment'))
            DROP INDEX [IX_EmployeePhotoProfileAttachment_EmployeeId_IsActive] ON [dbo].[EmployeePhotoProfileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeePhotoProfileAttachment_FileAttachmentId' AND object_id = OBJECT_ID('EmployeePhotoProfileAttachment'))
            DROP INDEX [IX_EmployeePhotoProfileAttachment_FileAttachmentId] ON [dbo].[EmployeePhotoProfileAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeePhotoProfileAttachment_EmployeeId' AND object_id = OBJECT_ID('EmployeePhotoProfileAttachment'))
            DROP INDEX [IX_EmployeePhotoProfileAttachment_EmployeeId] ON [dbo].[EmployeePhotoProfileAttachment];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EmployeePhotoProfileAttachment')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('EmployeePhotoProfileAttachment') AND name = 'FK_EmployeePhotoProfileAttachment_FileAttachment')
                ALTER TABLE [dbo].[EmployeePhotoProfileAttachment] DROP CONSTRAINT [FK_EmployeePhotoProfileAttachment_FileAttachment];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('EmployeePhotoProfileAttachment') AND name = 'FK_EmployeePhotoProfileAttachment_Employee')
                ALTER TABLE [dbo].[EmployeePhotoProfileAttachment] DROP CONSTRAINT [FK_EmployeePhotoProfileAttachment_Employee];
            
            DROP TABLE [dbo].[EmployeePhotoProfileAttachment];
        END
    ");
        }
    }
}