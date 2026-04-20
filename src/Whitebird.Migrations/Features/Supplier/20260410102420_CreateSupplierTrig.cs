using FluentMigrator;

namespace Whitebird.Migrations.Features.Supplier
{
    [Migration(20260410102420)]
    public class CreateSupplierTrig : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Trigger untuk update ModifiedDate
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Supplier_UpdateModifiedDate')
                    DROP TRIGGER TR_Supplier_UpdateModifiedDate;
                GO
                
                CREATE TRIGGER TR_Supplier_UpdateModifiedDate
                ON Supplier
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));
                    
                    UPDATE Supplier
                    SET ModifiedDate = GETDATE(),
                        ModifiedBy = ISNULL(@ContextInfo, 'System')
                    FROM Supplier s
                    INNER JOIN inserted i ON s.SupplierId = i.SupplierId;
                END;
                GO

                -- Trigger untuk prevent delete if has active Asset
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Supplier_PreventDeleteIfHasAsset')
                    DROP TRIGGER TR_Supplier_PreventDeleteIfHasAsset;
                GO
                
                CREATE TRIGGER TR_Supplier_PreventDeleteIfHasAsset
                ON Supplier
                INSTEAD OF DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));
                    
                    IF EXISTS (
                        SELECT 1 FROM Asset a
                        INNER JOIN deleted d ON a.SupplierId = d.SupplierId
                        WHERE a.IsActive = 1
                    )
                    BEGIN
                        RAISERROR('Cannot delete supplier that has active Asset', 16, 1);
                        RETURN;
                    END;
                    
                    -- Soft delete
                    UPDATE Supplier
                    SET IsActive = 0,
                        ModifiedDate = GETDATE(),
                        ModifiedBy = ISNULL(@ContextInfo, 'System')
                    FROM Supplier s
                    INNER JOIN deleted d ON s.SupplierId = d.SupplierId;
                END;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Supplier_UpdateModifiedDate')
                    DROP TRIGGER TR_Supplier_UpdateModifiedDate;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Supplier_PreventDeleteIfHasAsset')
                    DROP TRIGGER TR_Supplier_PreventDeleteIfHasAsset;
                GO
            ");
        }
    }
}