using FluentMigrator;

namespace Whitebird.Migrations.Features.AssetTransaction
{
    [Migration(20260410102416)]
    public class CreateAssetTransactionTrig : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Trigger untuk update ModifiedDate saja
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_AssetTransaction_UpdateModifiedDate')
                    DROP TRIGGER TR_AssetTransaction_UpdateModifiedDate;
                GO
                
                CREATE TRIGGER TR_AssetTransaction_UpdateModifiedDate
                ON AssetTransaction
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    UPDATE at
                    SET ModifiedDate = GETDATE()
                    FROM AssetTransaction at
                    INNER JOIN inserted i ON at.AssetTransactionId = i.AssetTransactionId;
                END;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_AssetTransaction_UpdateModifiedDate')
                    DROP TRIGGER TR_AssetTransaction_UpdateModifiedDate;
                GO
            ");
        }
    }
}