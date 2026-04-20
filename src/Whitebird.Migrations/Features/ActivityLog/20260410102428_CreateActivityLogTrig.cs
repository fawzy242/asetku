using FluentMigrator;

namespace Whitebird.Migrations.Features.ActivityLog
{
    [Migration(20260410102428)]
    public class CreateActivityLogTrig : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Trigger untuk auto-cleanup old logs (VERSION IMPROVED - Lighter)
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_ActivityLogs_AutoCleanup')
                    DROP TRIGGER TR_ActivityLogs_AutoCleanup;
                GO
                
                CREATE TRIGGER TR_ActivityLogs_AutoCleanup
                ON ActivityLogs
                AFTER INSERT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    -- Hanya cleanup jika jumlah row > 10000 (menggunakan metadata ringan)
                    DECLARE @RowCount BIGINT;
                    
                    SELECT @RowCount = SUM(rows)
                    FROM sys.partitions 
                    WHERE object_id = OBJECT_ID('ActivityLogs') 
                      AND index_id IN (0, 1);
                    
                    IF @RowCount > 10000
                    BEGIN
                        DELETE TOP (1000) FROM ActivityLogs
                        WHERE CreatedDate < DATEADD(YEAR, -1, GETDATE());
                    END;
                END;
                GO

                -- Trigger untuk prevent delete of recent logs (less than 30 days)
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_ActivityLogs_PreventDeleteRecent')
                    DROP TRIGGER TR_ActivityLogs_PreventDeleteRecent;
                GO
                
                CREATE TRIGGER TR_ActivityLogs_PreventDeleteRecent
                ON ActivityLogs
                INSTEAD OF DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    -- Allow delete only for logs older than 30 days
                    DELETE FROM ActivityLogs
                    WHERE LogId IN (SELECT LogId FROM deleted)
                    AND CreatedDate < DATEADD(DAY, -30, GETDATE());
                    
                    IF EXISTS (SELECT 1 FROM deleted WHERE CreatedDate >= DATEADD(DAY, -30, GETDATE()))
                    BEGIN
                        RAISERROR('Cannot delete activity logs less than 30 days old', 16, 1);
                    END;
                END;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_ActivityLogs_AutoCleanup')
                    DROP TRIGGER TR_ActivityLogs_AutoCleanup;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_ActivityLogs_PreventDeleteRecent')
                    DROP TRIGGER TR_ActivityLogs_PreventDeleteRecent;
                GO
            ");
        }
    }
}