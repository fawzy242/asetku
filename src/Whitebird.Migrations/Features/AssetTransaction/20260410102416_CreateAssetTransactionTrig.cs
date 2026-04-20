using FluentMigrator;

namespace Whitebird.Migrations.Features.AssetTransaction
{
    [Migration(20260410102416)]
    public class CreateAssetTransactionTrig : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Trigger untuk update status Asset saat transaksi selesai
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_AssetTransaction_UpdateAssetStatus')
                    DROP TRIGGER TR_AssetTransaction_UpdateAssetStatus;
                GO
                
                CREATE TRIGGER TR_AssetTransaction_UpdateAssetStatus
                ON AssetTransaction
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));
                    
                    -- Update Asset status when transaction is completed
                    IF UPDATE(TransactionStatus)
                    BEGIN
                        UPDATE a
                        SET a.Status = CASE 
                            WHEN i.TransactionStatus = 'Completed' AND i.TransactionType = 'Assignment' THEN 'Assigned'
                            WHEN i.TransactionStatus = 'Completed' AND i.TransactionType = 'Return' THEN 'Available'
                            WHEN i.TransactionStatus = 'Completed' AND i.TransactionType = 'Maintenance' THEN 'Maintenance'
                            WHEN i.TransactionStatus = 'Completed' AND i.TransactionType = 'Disposal' THEN 'Disposed'
                            ELSE a.Status
                        END,
                        a.ModifiedDate = GETDATE(),
                        a.ModifiedBy = ISNULL(@ContextInfo, 'System')
                        FROM Asset a
                        INNER JOIN inserted i ON a.AssetId = i.AssetId
                        WHERE i.TransactionStatus = 'Completed'
                          AND i.TransactionType IN ('Assignment', 'Return', 'Maintenance', 'Disposal');
                    END;
                END;
                GO

                -- Trigger untuk update ModifiedDate
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_AssetTransaction_UpdateModifiedDate')
                    DROP TRIGGER TR_AssetTransaction_UpdateModifiedDate;
                GO
                
                CREATE TRIGGER TR_AssetTransaction_UpdateModifiedDate
                ON AssetTransaction
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));
                    
                    UPDATE AssetTransaction
                    SET ModifiedDate = GETDATE(),
                        ModifiedBy = ISNULL(@ContextInfo, 'System')
                    FROM AssetTransaction at
                    INNER JOIN inserted i ON at.AssetTransactionId = i.AssetTransactionId;
                END;
                GO

                -- Trigger untuk audit log transaksi
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_AssetTransaction_AuditLog')
                    DROP TRIGGER TR_AssetTransaction_AuditLog;
                GO
                
                CREATE TRIGGER TR_AssetTransaction_AuditLog
                ON AssetTransaction
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));
                    
                    -- Log untuk INSERT
                    IF EXISTS (SELECT * FROM inserted) AND NOT EXISTS (SELECT * FROM deleted)
                    BEGIN
                        INSERT INTO ActivityLogs (
                            ReferenceTable,
                            ReferenceId,
                            ActivityType,
                            Description,
                            CreatedDate,
                            CreatedBy
                        )
                        SELECT 
                            'AssetTransaction',
                            i.AssetTransactionId,
                            'CREATE',
                            'Transaction created: ' + i.TransactionType + ' for AssetId ' + CAST(i.AssetId AS NVARCHAR(10)),
                            GETDATE(),
                            ISNULL(@ContextInfo, 'System')
                        FROM inserted i;
                    END;
                    
                    -- Log untuk UPDATE status
                    IF EXISTS (SELECT * FROM inserted) AND EXISTS (SELECT * FROM deleted)
                    BEGIN
                        INSERT INTO ActivityLogs (
                            ReferenceTable,
                            ReferenceId,
                            ActivityType,
                            Description,
                            CreatedDate,
                            CreatedBy
                        )
                        SELECT 
                            'AssetTransaction',
                            i.AssetTransactionId,
                            'UPDATE',
                            'Transaction status changed from ' + ISNULL(d.TransactionStatus, 'NULL') + ' to ' + i.TransactionStatus,
                            GETDATE(),
                            ISNULL(@ContextInfo, 'System')
                        FROM inserted i
                        INNER JOIN deleted d ON i.AssetTransactionId = d.AssetTransactionId
                        WHERE i.TransactionStatus != d.TransactionStatus;
                    END;
                END;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_AssetTransaction_UpdateAssetStatus')
                    DROP TRIGGER TR_AssetTransaction_UpdateAssetStatus;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_AssetTransaction_UpdateModifiedDate')
                    DROP TRIGGER TR_AssetTransaction_UpdateModifiedDate;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_AssetTransaction_AuditLog')
                    DROP TRIGGER TR_AssetTransaction_AuditLog;
                GO
            ");
        }
    }
}