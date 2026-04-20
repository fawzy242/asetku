using FluentMigrator;

namespace Whitebird.Migrations.Features.ActivityLog
{
    [Migration(20260410102411)]
    public class CreateActivityLogTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'ActivityLogs' AND xtype = 'U')
                BEGIN
                    CREATE TABLE ActivityLogs (
                        LogId INT NOT NULL CONSTRAINT DF_ActivityLogs_LogId DEFAULT NEXT VALUE FOR Seq_LogId,
                        ReferenceTable NVARCHAR(100) NOT NULL,
                        ReferenceId INT NOT NULL,
                        ActivityType NVARCHAR(50) NOT NULL,
                        Description NVARCHAR(500) NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_ActivityLogs_IsActive DEFAULT 1,
                        CreatedDate DATETIME NOT NULL CONSTRAINT DF_ActivityLogs_CreatedDate DEFAULT GETDATE(),
                        CreatedBy NVARCHAR(50) NOT NULL CONSTRAINT DF_ActivityLogs_CreatedBy DEFAULT 'System',
                        ModifiedDate DATETIME NULL,
                        ModifiedBy NVARCHAR(50) NULL,
                        
                        CONSTRAINT PK_ActivityLogs PRIMARY KEY (LogId)
                    );
                END;
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLogs_ReferenceTable_ReferenceId' AND object_id = OBJECT_ID('ActivityLogs'))
                    CREATE INDEX IX_ActivityLogs_ReferenceTable_ReferenceId ON ActivityLogs(ReferenceTable, ReferenceId);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLogs_ActivityType' AND object_id = OBJECT_ID('ActivityLogs'))
                    CREATE INDEX IX_ActivityLogs_ActivityType ON ActivityLogs(ActivityType);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ActivityLogs_CreatedDate' AND object_id = OBJECT_ID('ActivityLogs'))
                    CREATE INDEX IX_ActivityLogs_CreatedDate ON ActivityLogs(CreatedDate DESC);
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name = 'ActivityLogs' AND xtype = 'U')
                    DROP TABLE ActivityLogs;
                GO
            ");
        }
    }
}
