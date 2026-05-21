using FluentMigrator;

namespace Whitebird.Migrations.Features.MasterData
{
    [Migration(20260519213252)]
    public class CreateTableMasterData : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MasterData')
        BEGIN
            CREATE TABLE [dbo].[MasterData] (
                [MasterDataId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_MasterData_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_MasterData_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_MasterData_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [ReferenceCode] NVARCHAR(100) NOT NULL,
                [ReferenceName] NVARCHAR(200) NOT NULL,
                [MasterDataName] NVARCHAR(200) NULL,
                CONSTRAINT [PK_MasterData] PRIMARY KEY ([MasterDataId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES (menggunakan OBJECT_ID yang lebih aman)
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MasterData_ReferenceCode_ReferenceName' AND object_id = OBJECT_ID('MasterData'))
            CREATE INDEX [IX_MasterData_ReferenceCode_ReferenceName]
            ON [dbo].[MasterData]([ReferenceCode], [ReferenceName]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MasterData_ReferenceName' AND object_id = OBJECT_ID('MasterData'))
            CREATE INDEX [IX_MasterData_ReferenceName]
            ON [dbo].[MasterData]([ReferenceName]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MasterData_IsActive' AND object_id = OBJECT_ID('MasterData'))
            CREATE INDEX [IX_MasterData_IsActive]
            ON [dbo].[MasterData]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MasterData_IsActive' AND object_id = OBJECT_ID('MasterData'))
            DROP INDEX [IX_MasterData_IsActive] ON [dbo].[MasterData];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MasterData_ReferenceName' AND object_id = OBJECT_ID('MasterData'))
            DROP INDEX [IX_MasterData_ReferenceName] ON [dbo].[MasterData];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MasterData_ReferenceCode_ReferenceName' AND object_id = OBJECT_ID('MasterData'))
            DROP INDEX [IX_MasterData_ReferenceCode_ReferenceName] ON [dbo].[MasterData];

        -- ============================================================
        -- DROP TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'MasterData')
            DROP TABLE [dbo].[MasterData];
    ");
        }
    }
}