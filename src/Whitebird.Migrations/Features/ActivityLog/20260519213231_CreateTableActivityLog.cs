using FluentMigrator;

namespace Whitebird.Migrations.Features.ActivityLog
{
    [Migration(20260519213231)]
    public class CreateTableActivityLog : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ActivityLogs')
        BEGIN
            CREATE TABLE [dbo].[ActivityLogs] (
                [LogId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_ActivityLogs_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_ActivityLogs_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_ActivityLogs_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [ReferenceTable] NVARCHAR(100) NOT NULL,
                [ReferenceId] INT NOT NULL,
                [ActivityType] NVARCHAR(50) NOT NULL,
                [Description] NVARCHAR(500) NULL,
                CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([LogId])
            );
        END;

        -- ============================================================
        -- CREATE INDEXES (menggunakan sys.indexes dengan EXISTS check yang aman)
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ActivityLogs_ReferenceTable_ReferenceId' AND object_id = OBJECT_ID('ActivityLogs'))
            CREATE INDEX [IX_ActivityLogs_ReferenceTable_ReferenceId]
            ON [dbo].[ActivityLogs]([ReferenceTable], [ReferenceId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ActivityLogs_CreatedDate' AND object_id = OBJECT_ID('ActivityLogs'))
            CREATE INDEX [IX_ActivityLogs_CreatedDate]
            ON [dbo].[ActivityLogs]([CreatedDate] DESC);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ActivityLogs_ActivityType' AND object_id = OBJECT_ID('ActivityLogs'))
            CREATE INDEX [IX_ActivityLogs_ActivityType]
            ON [dbo].[ActivityLogs]([ActivityType]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ActivityLogs_IsActive' AND object_id = OBJECT_ID('ActivityLogs'))
            CREATE INDEX [IX_ActivityLogs_IsActive]
            ON [dbo].[ActivityLogs]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ActivityLogs_IsActive' AND object_id = OBJECT_ID('ActivityLogs'))
            DROP INDEX [IX_ActivityLogs_IsActive] ON [dbo].[ActivityLogs];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ActivityLogs_ActivityType' AND object_id = OBJECT_ID('ActivityLogs'))
            DROP INDEX [IX_ActivityLogs_ActivityType] ON [dbo].[ActivityLogs];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ActivityLogs_CreatedDate' AND object_id = OBJECT_ID('ActivityLogs'))
            DROP INDEX [IX_ActivityLogs_CreatedDate] ON [dbo].[ActivityLogs];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ActivityLogs_ReferenceTable_ReferenceId' AND object_id = OBJECT_ID('ActivityLogs'))
            DROP INDEX [IX_ActivityLogs_ReferenceTable_ReferenceId] ON [dbo].[ActivityLogs];

        -- ============================================================
        -- DROP TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ActivityLogs')
            DROP TABLE [dbo].[ActivityLogs];
    ");
        }
    }
}