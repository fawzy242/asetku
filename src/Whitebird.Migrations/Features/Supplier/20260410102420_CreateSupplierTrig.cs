using FluentMigrator;

namespace Whitebird.Migrations.Features.Supplier
{
    [Migration(20260410102420)]
    public class CreateSupplierTrig : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Supplier_UpdateModifiedDate')
                    DROP TRIGGER TR_Supplier_UpdateModifiedDate;
                GO
                
                CREATE TRIGGER TR_Supplier_UpdateModifiedDate
                ON Supplier
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    UPDATE s
                    SET ModifiedDate = GETDATE()
                    FROM Supplier s
                    INNER JOIN inserted i ON s.SupplierId = i.SupplierId;
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
            ");
        }
    }
}