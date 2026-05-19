using FluentMigrator;

namespace Whitebird.Migrations.Features.ActivityLog
{
    [Migration(20260519213231)]
    public class CreateTableActivityLog : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        IF NOT EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'ActivityLogs' AND [xtype] = 'U')
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

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_ActivityLogs_ReferenceTable_ReferenceId')
            CREATE INDEX [IX_ActivityLogs_ReferenceTable_ReferenceId]
            ON [dbo].[ActivityLogs]([ReferenceTable], [ReferenceId]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_ActivityLogs_CreatedDate')
            CREATE INDEX [IX_ActivityLogs_CreatedDate]
            ON [dbo].[ActivityLogs]([CreatedDate] DESC);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_ActivityLogs_ActivityType')
            CREATE INDEX [IX_ActivityLogs_ActivityType]
            ON [dbo].[ActivityLogs]([ActivityType]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_ActivityLogs_IsActive')
            CREATE INDEX [IX_ActivityLogs_IsActive]
            ON [dbo].[ActivityLogs]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_ActivityLogs_IsActive')
            DROP INDEX [IX_ActivityLogs_IsActive] ON [dbo].[ActivityLogs];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_ActivityLogs_ActivityType')
            DROP INDEX [IX_ActivityLogs_ActivityType] ON [dbo].[ActivityLogs];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_ActivityLogs_CreatedDate')
            DROP INDEX [IX_ActivityLogs_CreatedDate] ON [dbo].[ActivityLogs];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_ActivityLogs_ReferenceTable_ReferenceId')
            DROP INDEX [IX_ActivityLogs_ReferenceTable_ReferenceId] ON [dbo].[ActivityLogs];

        IF EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'ActivityLogs' AND [xtype] = 'U')
            DROP TABLE [dbo].[ActivityLogs];
    ");
        }
    }
}
