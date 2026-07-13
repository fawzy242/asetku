using FluentMigrator;

namespace Whitebird.Migrations.Features.Schedule
{
    [Migration(20260713205435)]
    public class CreateTableScheduleMaintenance : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ScheduleMaintenance')
        BEGIN
            CREATE TABLE [dbo].[ScheduleMaintenance] (
                [ScheduleMaintenanceId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_ScheduleMaintenance_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_ScheduleMaintenance_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ScheduleMaintenance_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [ScheduleDate] DATETIME NOT NULL,
                [AssetId] INT NOT NULL,
                [MaintenanceType] INT NOT NULL,
                [MaintenanceAssetTransactionId] INT NULL,
                [PostAssetTransactionId] INT NULL,
                CONSTRAINT [PK_ScheduleMaintenance] PRIMARY KEY ([ScheduleMaintenanceId]),
                CONSTRAINT [FK_ScheduleMaintenance_Asset]
                    FOREIGN KEY ([AssetId]) REFERENCES [dbo].[Asset]([AssetId]),
                CONSTRAINT [FK_ScheduleMaintenance_MaintenanceAssetTransaction]
                    FOREIGN KEY ([MaintenanceAssetTransactionId]) REFERENCES [dbo].[AssetTransaction]([AssetTransactionId]),
                CONSTRAINT [FK_ScheduleMaintenance_PostAssetTransaction]
                    FOREIGN KEY ([PostAssetTransactionId]) REFERENCES [dbo].[AssetTransaction]([AssetTransactionId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_ScheduleDate' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            CREATE INDEX [IX_ScheduleMaintenance_ScheduleDate]
            ON [dbo].[ScheduleMaintenance]([ScheduleDate] DESC);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_AssetId' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            CREATE INDEX [IX_ScheduleMaintenance_AssetId]
            ON [dbo].[ScheduleMaintenance]([AssetId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_MaintenanceType' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            CREATE INDEX [IX_ScheduleMaintenance_MaintenanceType]
            ON [dbo].[ScheduleMaintenance]([MaintenanceType]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_MaintenanceAssetTransactionId' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            CREATE INDEX [IX_ScheduleMaintenance_MaintenanceAssetTransactionId]
            ON [dbo].[ScheduleMaintenance]([MaintenanceAssetTransactionId])
            WHERE [MaintenanceAssetTransactionId] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_PostAssetTransactionId' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            CREATE INDEX [IX_ScheduleMaintenance_PostAssetTransactionId]
            ON [dbo].[ScheduleMaintenance]([PostAssetTransactionId])
            WHERE [PostAssetTransactionId] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_IsActive' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            CREATE INDEX [IX_ScheduleMaintenance_IsActive]
            ON [dbo].[ScheduleMaintenance]([IsActive])
            WHERE [IsActive] = 1;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_AssetId_ScheduleDate' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            CREATE INDEX [IX_ScheduleMaintenance_AssetId_ScheduleDate]
            ON [dbo].[ScheduleMaintenance]([AssetId], [ScheduleDate] DESC);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_MaintenanceType_ScheduleDate' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            CREATE INDEX [IX_ScheduleMaintenance_MaintenanceType_ScheduleDate]
            ON [dbo].[ScheduleMaintenance]([MaintenanceType], [ScheduleDate] DESC);
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_MaintenanceType_ScheduleDate' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            DROP INDEX [IX_ScheduleMaintenance_MaintenanceType_ScheduleDate] ON [dbo].[ScheduleMaintenance];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_AssetId_ScheduleDate' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            DROP INDEX [IX_ScheduleMaintenance_AssetId_ScheduleDate] ON [dbo].[ScheduleMaintenance];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_IsActive' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            DROP INDEX [IX_ScheduleMaintenance_IsActive] ON [dbo].[ScheduleMaintenance];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_PostAssetTransactionId' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            DROP INDEX [IX_ScheduleMaintenance_PostAssetTransactionId] ON [dbo].[ScheduleMaintenance];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_MaintenanceAssetTransactionId' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            DROP INDEX [IX_ScheduleMaintenance_MaintenanceAssetTransactionId] ON [dbo].[ScheduleMaintenance];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_MaintenanceType' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            DROP INDEX [IX_ScheduleMaintenance_MaintenanceType] ON [dbo].[ScheduleMaintenance];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_AssetId' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            DROP INDEX [IX_ScheduleMaintenance_AssetId] ON [dbo].[ScheduleMaintenance];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ScheduleMaintenance_ScheduleDate' AND object_id = OBJECT_ID('ScheduleMaintenance'))
            DROP INDEX [IX_ScheduleMaintenance_ScheduleDate] ON [dbo].[ScheduleMaintenance];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ScheduleMaintenance')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('ScheduleMaintenance') AND name = 'FK_ScheduleMaintenance_PostAssetTransaction')
                ALTER TABLE [dbo].[ScheduleMaintenance] DROP CONSTRAINT [FK_ScheduleMaintenance_PostAssetTransaction];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('ScheduleMaintenance') AND name = 'FK_ScheduleMaintenance_MaintenanceAssetTransaction')
                ALTER TABLE [dbo].[ScheduleMaintenance] DROP CONSTRAINT [FK_ScheduleMaintenance_MaintenanceAssetTransaction];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('ScheduleMaintenance') AND name = 'FK_ScheduleMaintenance_Asset')
                ALTER TABLE [dbo].[ScheduleMaintenance] DROP CONSTRAINT [FK_ScheduleMaintenance_Asset];
            
            DROP TABLE [dbo].[ScheduleMaintenance];
        END
    ");
        }
    }
}