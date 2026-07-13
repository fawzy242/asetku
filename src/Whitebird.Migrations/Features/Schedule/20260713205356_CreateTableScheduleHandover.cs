using FluentMigrator;

namespace Whitebird.Migrations.Features.Schedule
{
    [Migration(20260713205356)]
    public class CreateTableScheduleHandover : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ScheduleHandover')
        BEGIN
            CREATE TABLE [dbo].[ScheduleHandover] (
                [ScheduleHandoverId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_ScheduleHandover_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_ScheduleHandover_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ScheduleHandover_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [ScheduleDate] DATETIME NOT NULL,
                [AssetId] INT NOT NULL,
                [EmployeeId] INT NOT NULL,
                [HandoverAssetTransactionId] INT NULL,
                CONSTRAINT [PK_ScheduleHandover] PRIMARY KEY ([ScheduleHandoverId]),
                CONSTRAINT [FK_ScheduleHandover_Asset]
                    FOREIGN KEY ([AssetId]) REFERENCES [dbo].[Asset]([AssetId]),
                CONSTRAINT [FK_ScheduleHandover_Employee]
                    FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employee]([EmployeeId]),
                CONSTRAINT [FK_ScheduleHandover_AssetTransaction]
                    FOREIGN KEY ([HandoverAssetTransactionId]) REFERENCES [dbo].[AssetTransaction]([AssetTransactionId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleHandover_ScheduleDate' AND object_id = OBJECT_ID('ScheduleHandover'))
            CREATE INDEX [IX_ScheduleHandover_ScheduleDate]
            ON [dbo].[ScheduleHandover]([ScheduleDate] DESC);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleHandover_AssetId' AND object_id = OBJECT_ID('ScheduleHandover'))
            CREATE INDEX [IX_ScheduleHandover_AssetId]
            ON [dbo].[ScheduleHandover]([AssetId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleHandover_EmployeeId' AND object_id = OBJECT_ID('ScheduleHandover'))
            CREATE INDEX [IX_ScheduleHandover_EmployeeId]
            ON [dbo].[ScheduleHandover]([EmployeeId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleHandover_HandoverAssetTransactionId' AND object_id = OBJECT_ID('ScheduleHandover'))
            CREATE INDEX [IX_ScheduleHandover_HandoverAssetTransactionId]
            ON [dbo].[ScheduleHandover]([HandoverAssetTransactionId])
            WHERE [HandoverAssetTransactionId] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleHandover_IsActive' AND object_id = OBJECT_ID('ScheduleHandover'))
            CREATE INDEX [IX_ScheduleHandover_IsActive]
            ON [dbo].[ScheduleHandover]([IsActive])
            WHERE [IsActive] = 1;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleHandover_AssetId_ScheduleDate' AND object_id = OBJECT_ID('ScheduleHandover'))
            CREATE INDEX [IX_ScheduleHandover_AssetId_ScheduleDate]
            ON [dbo].[ScheduleHandover]([AssetId], [ScheduleDate] DESC);
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleHandover_AssetId_ScheduleDate' AND object_id = OBJECT_ID('ScheduleHandover'))
            DROP INDEX [IX_ScheduleHandover_AssetId_ScheduleDate] ON [dbo].[ScheduleHandover];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleHandover_IsActive' AND object_id = OBJECT_ID('ScheduleHandover'))
            DROP INDEX [IX_ScheduleHandover_IsActive] ON [dbo].[ScheduleHandover];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleHandover_HandoverAssetTransactionId' AND object_id = OBJECT_ID('ScheduleHandover'))
            DROP INDEX [IX_ScheduleHandover_HandoverAssetTransactionId] ON [dbo].[ScheduleHandover];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleHandover_EmployeeId' AND object_id = OBJECT_ID('ScheduleHandover'))
            DROP INDEX [IX_ScheduleHandover_EmployeeId] ON [dbo].[ScheduleHandover];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleHandover_AssetId' AND object_id = OBJECT_ID('ScheduleHandover'))
            DROP INDEX [IX_ScheduleHandover_AssetId] ON [dbo].[ScheduleHandover];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleHandover_ScheduleDate' AND object_id = OBJECT_ID('ScheduleHandover'))
            DROP INDEX [IX_ScheduleHandover_ScheduleDate] ON [dbo].[ScheduleHandover];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ScheduleHandover')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('ScheduleHandover') AND name = 'FK_ScheduleHandover_AssetTransaction')
                ALTER TABLE [dbo].[ScheduleHandover] DROP CONSTRAINT [FK_ScheduleHandover_AssetTransaction];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('ScheduleHandover') AND name = 'FK_ScheduleHandover_Employee')
                ALTER TABLE [dbo].[ScheduleHandover] DROP CONSTRAINT [FK_ScheduleHandover_Employee];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('ScheduleHandover') AND name = 'FK_ScheduleHandover_Asset')
                ALTER TABLE [dbo].[ScheduleHandover] DROP CONSTRAINT [FK_ScheduleHandover_Asset];
            
            DROP TABLE [dbo].[ScheduleHandover];
        END
    ");
        }
    }
}