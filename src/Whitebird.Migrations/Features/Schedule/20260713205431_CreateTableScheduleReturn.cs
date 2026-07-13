using FluentMigrator;

namespace Whitebird.Migrations.Features.Schedule
{
    [Migration(20260713205431)]
    public class CreateTableScheduleReturn : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ScheduleReturn')
        BEGIN
            CREATE TABLE [dbo].[ScheduleReturn] (
                [ScheduleReturnId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_ScheduleReturn_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_ScheduleReturn_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ScheduleReturn_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [ScheduleDate] DATETIME NOT NULL,
                [AssetId] INT NOT NULL,
                [EmployeeId] INT NOT NULL,
                [ReturnAssetTransactionId] INT NULL,
                CONSTRAINT [PK_ScheduleReturn] PRIMARY KEY ([ScheduleReturnId]),
                CONSTRAINT [FK_ScheduleReturn_Asset]
                    FOREIGN KEY ([AssetId]) REFERENCES [dbo].[Asset]([AssetId]),
                CONSTRAINT [FK_ScheduleReturn_Employee]
                    FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employee]([EmployeeId]),
                CONSTRAINT [FK_ScheduleReturn_AssetTransaction]
                    FOREIGN KEY ([ReturnAssetTransactionId]) REFERENCES [dbo].[AssetTransaction]([AssetTransactionId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleReturn_ScheduleDate' AND object_id = OBJECT_ID('ScheduleReturn'))
            CREATE INDEX [IX_ScheduleReturn_ScheduleDate]
            ON [dbo].[ScheduleReturn]([ScheduleDate] DESC);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleReturn_AssetId' AND object_id = OBJECT_ID('ScheduleReturn'))
            CREATE INDEX [IX_ScheduleReturn_AssetId]
            ON [dbo].[ScheduleReturn]([AssetId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleReturn_EmployeeId' AND object_id = OBJECT_ID('ScheduleReturn'))
            CREATE INDEX [IX_ScheduleReturn_EmployeeId]
            ON [dbo].[ScheduleReturn]([EmployeeId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleReturn_ReturnAssetTransactionId' AND object_id = OBJECT_ID('ScheduleReturn'))
            CREATE INDEX [IX_ScheduleReturn_ReturnAssetTransactionId]
            ON [dbo].[ScheduleReturn]([ReturnAssetTransactionId])
            WHERE [ReturnAssetTransactionId] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleReturn_IsActive' AND object_id = OBJECT_ID('ScheduleReturn'))
            CREATE INDEX [IX_ScheduleReturn_IsActive]
            ON [dbo].[ScheduleReturn]([IsActive])
            WHERE [IsActive] = 1;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleReturn_AssetId_ScheduleDate' AND object_id = OBJECT_ID('ScheduleReturn'))
            CREATE INDEX [IX_ScheduleReturn_AssetId_ScheduleDate]
            ON [dbo].[ScheduleReturn]([AssetId], [ScheduleDate] DESC);
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleReturn_AssetId_ScheduleDate' AND object_id = OBJECT_ID('ScheduleReturn'))
            DROP INDEX [IX_ScheduleReturn_AssetId_ScheduleDate] ON [dbo].[ScheduleReturn];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleReturn_IsActive' AND object_id = OBJECT_ID('ScheduleReturn'))
            DROP INDEX [IX_ScheduleReturn_IsActive] ON [dbo].[ScheduleReturn];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleReturn_ReturnAssetTransactionId' AND object_id = OBJECT_ID('ScheduleReturn'))
            DROP INDEX [IX_ScheduleReturn_ReturnAssetTransactionId] ON [dbo].[ScheduleReturn];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleReturn_EmployeeId' AND object_id = OBJECT_ID('ScheduleReturn'))
            DROP INDEX [IX_ScheduleReturn_EmployeeId] ON [dbo].[ScheduleReturn];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleReturn_AssetId' AND object_id = OBJECT_ID('ScheduleReturn'))
            DROP INDEX [IX_ScheduleReturn_AssetId] ON [dbo].[ScheduleReturn];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleReturn_ScheduleDate' AND object_id = OBJECT_ID('ScheduleReturn'))
            DROP INDEX [IX_ScheduleReturn_ScheduleDate] ON [dbo].[ScheduleReturn];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ScheduleReturn')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('ScheduleReturn') AND name = 'FK_ScheduleReturn_AssetTransaction')
                ALTER TABLE [dbo].[ScheduleReturn] DROP CONSTRAINT [FK_ScheduleReturn_AssetTransaction];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('ScheduleReturn') AND name = 'FK_ScheduleReturn_Employee')
                ALTER TABLE [dbo].[ScheduleReturn] DROP CONSTRAINT [FK_ScheduleReturn_Employee];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('ScheduleReturn') AND name = 'FK_ScheduleReturn_Asset')
                ALTER TABLE [dbo].[ScheduleReturn] DROP CONSTRAINT [FK_ScheduleReturn_Asset];
            
            DROP TABLE [dbo].[ScheduleReturn];
        END
    ");
        }
    }
}