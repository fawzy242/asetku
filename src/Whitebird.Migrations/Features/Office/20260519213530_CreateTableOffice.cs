using FluentMigrator;

namespace Whitebird.Migrations.Features.Office
{
    [Migration(20260519213530)]
    public class CreateTableOffice : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Office')
        BEGIN
            CREATE TABLE [dbo].[Office] (
                [OfficeId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_Office_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_Office_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Office_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [OfficeCode] NVARCHAR(50) NULL,
                [OfficeName] NVARCHAR(100) NULL,
                [OfficeType] INT NULL,
                [City] NVARCHAR(100) NULL,
                [Address] NVARCHAR(500) NULL,
                [Phone] NVARCHAR(50) NULL,
                [ParentOfficeId] INT NULL,
                CONSTRAINT [PK_Office] PRIMARY KEY ([OfficeId]),
                CONSTRAINT [UQ_Office_OfficeCode] UNIQUE ([OfficeCode]),
                CONSTRAINT [FK_Office_ParentOffice]
                    FOREIGN KEY ([ParentOfficeId]) REFERENCES [dbo].[Office]([OfficeId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES (menggunakan OBJECT_ID yang lebih aman)
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Office_OfficeName' AND object_id = OBJECT_ID('Office'))
            CREATE INDEX [IX_Office_OfficeName]
            ON [dbo].[Office]([OfficeName]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Office_OfficeType' AND object_id = OBJECT_ID('Office'))
            CREATE INDEX [IX_Office_OfficeType]
            ON [dbo].[Office]([OfficeType]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Office_City' AND object_id = OBJECT_ID('Office'))
            CREATE INDEX [IX_Office_City]
            ON [dbo].[Office]([City]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Office_ParentOfficeId' AND object_id = OBJECT_ID('Office'))
            CREATE INDEX [IX_Office_ParentOfficeId]
            ON [dbo].[Office]([ParentOfficeId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Office_IsActive' AND object_id = OBJECT_ID('Office'))
            CREATE INDEX [IX_Office_IsActive]
            ON [dbo].[Office]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Office_IsActive' AND object_id = OBJECT_ID('Office'))
            DROP INDEX [IX_Office_IsActive] ON [dbo].[Office];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Office_ParentOfficeId' AND object_id = OBJECT_ID('Office'))
            DROP INDEX [IX_Office_ParentOfficeId] ON [dbo].[Office];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Office_City' AND object_id = OBJECT_ID('Office'))
            DROP INDEX [IX_Office_City] ON [dbo].[Office];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Office_OfficeType' AND object_id = OBJECT_ID('Office'))
            DROP INDEX [IX_Office_OfficeType] ON [dbo].[Office];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Office_OfficeName' AND object_id = OBJECT_ID('Office'))
            DROP INDEX [IX_Office_OfficeName] ON [dbo].[Office];

        -- ============================================================
        -- DROP FOREIGN KEY & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Office')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('Office') AND name = 'FK_Office_ParentOffice')
                ALTER TABLE [dbo].[Office] DROP CONSTRAINT [FK_Office_ParentOffice];
            
            DROP TABLE [dbo].[Office];
        END
    ");
        }
    }
}