using FluentMigrator;

namespace Whitebird.Migrations.Features.Employee
{
    [Migration(20260713205141)]
    public class CreateTableEmployeeAttachment : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EmployeeAttachment')
        BEGIN
            CREATE TABLE [dbo].[EmployeeAttachment] (
                [EmployeeAttachmentId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_EmployeeAttachment_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_EmployeeAttachment_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_EmployeeAttachment_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [EmployeeId] INT NOT NULL,
                [FileAttachmentId] INT NOT NULL,
                [Order] INT NOT NULL CONSTRAINT [DF_EmployeeAttachment_Order] DEFAULT 0,
                CONSTRAINT [PK_EmployeeAttachment] PRIMARY KEY ([EmployeeAttachmentId]),
                CONSTRAINT [FK_EmployeeAttachment_Employee]
                    FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employee]([EmployeeId]),
                CONSTRAINT [FK_EmployeeAttachment_FileAttachment]
                    FOREIGN KEY ([FileAttachmentId]) REFERENCES [dbo].[FileAttachment]([FileAttachmentId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeAttachment_EmployeeId' AND object_id = OBJECT_ID('EmployeeAttachment'))
            CREATE INDEX [IX_EmployeeAttachment_EmployeeId]
            ON [dbo].[EmployeeAttachment]([EmployeeId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeAttachment_FileAttachmentId' AND object_id = OBJECT_ID('EmployeeAttachment'))
            CREATE INDEX [IX_EmployeeAttachment_FileAttachmentId]
            ON [dbo].[EmployeeAttachment]([FileAttachmentId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeAttachment_EmployeeId_Order' AND object_id = OBJECT_ID('EmployeeAttachment'))
            CREATE INDEX [IX_EmployeeAttachment_EmployeeId_Order]
            ON [dbo].[EmployeeAttachment]([EmployeeId], [Order]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeAttachment_IsActive' AND object_id = OBJECT_ID('EmployeeAttachment'))
            CREATE INDEX [IX_EmployeeAttachment_IsActive]
            ON [dbo].[EmployeeAttachment]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeAttachment_IsActive' AND object_id = OBJECT_ID('EmployeeAttachment'))
            DROP INDEX [IX_EmployeeAttachment_IsActive] ON [dbo].[EmployeeAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeAttachment_EmployeeId_Order' AND object_id = OBJECT_ID('EmployeeAttachment'))
            DROP INDEX [IX_EmployeeAttachment_EmployeeId_Order] ON [dbo].[EmployeeAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeAttachment_FileAttachmentId' AND object_id = OBJECT_ID('EmployeeAttachment'))
            DROP INDEX [IX_EmployeeAttachment_FileAttachmentId] ON [dbo].[EmployeeAttachment];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmployeeAttachment_EmployeeId' AND object_id = OBJECT_ID('EmployeeAttachment'))
            DROP INDEX [IX_EmployeeAttachment_EmployeeId] ON [dbo].[EmployeeAttachment];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'EmployeeAttachment')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('EmployeeAttachment') AND name = 'FK_EmployeeAttachment_FileAttachment')
                ALTER TABLE [dbo].[EmployeeAttachment] DROP CONSTRAINT [FK_EmployeeAttachment_FileAttachment];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('EmployeeAttachment') AND name = 'FK_EmployeeAttachment_Employee')
                ALTER TABLE [dbo].[EmployeeAttachment] DROP CONSTRAINT [FK_EmployeeAttachment_Employee];
            
            DROP TABLE [dbo].[EmployeeAttachment];
        END
    ");
        }
    }
}