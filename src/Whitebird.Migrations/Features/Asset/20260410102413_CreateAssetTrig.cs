using FluentMigrator;

namespace Whitebird.Migrations.Features.Asset
{
    [Migration(20260410102413)]
    public class CreateAssetTrig : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Trigger untuk auto-generate AssetCode
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Asset_GenerateAssetCode')
                    DROP TRIGGER TR_Asset_GenerateAssetCode;
                GO
                
                CREATE TRIGGER TR_Asset_GenerateAssetCode
                ON Asset
                INSTEAD OF INSERT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    DECLARE @NewCode NVARCHAR(50);
                    
                    INSERT INTO Asset (
                        AssetId, AssetCode, AssetName, CategoryId, SubCategory, AssetType,
                        Brand, Model, SerialNumber, Imei, MacAddress,
                        PurchaseDate, PurchasePrice, InvoiceNumber, SupplierId,
                        WarrantyPeriod, WarrantyExpiryDate, Condition, Status,
                        Location, CurrentHolderId, ResponsiblePartyId, ResidualValue,
                        UsefulLife, DepreciationStartDate, Notes, LastMaintenanceDate,
                        NextMaintenanceDate, IsActive, CreatedDate, CreatedBy,
                        ModifiedDate, ModifiedBy
                    )
                    SELECT 
                        NEXT VALUE FOR Seq_AssetId,
                        CASE 
                            WHEN i.AssetCode IS NULL OR i.AssetCode = '' 
                            THEN 'AST-' + RIGHT('000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000 AS NVARCHAR(6)), 6)
                            ELSE i.AssetCode
                        END,
                        i.AssetName, i.CategoryId, i.SubCategory, i.AssetType,
                        i.Brand, i.Model, i.SerialNumber, i.Imei, i.MacAddress,
                        i.PurchaseDate, i.PurchasePrice, i.InvoiceNumber, i.SupplierId,
                        i.WarrantyPeriod, i.WarrantyExpiryDate, i.Condition, i.Status,
                        i.Location, i.CurrentHolderId, i.ResponsiblePartyId, i.ResidualValue,
                        i.UsefulLife, i.DepreciationStartDate, i.Notes, i.LastMaintenanceDate,
                        i.NextMaintenanceDate, i.IsActive, i.CreatedDate, i.CreatedBy,
                        i.ModifiedDate, i.ModifiedBy
                    FROM inserted i;
                END;
                GO

                -- Trigger untuk update ModifiedDate
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Asset_UpdateModifiedDate')
                    DROP TRIGGER TR_Asset_UpdateModifiedDate;
                GO
                
                CREATE TRIGGER TR_Asset_UpdateModifiedDate
                ON Asset
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));
                    
                    UPDATE Asset
                    SET ModifiedDate = GETDATE(),
                        ModifiedBy = ISNULL(@ContextInfo, 'System')
                    FROM Asset a
                    INNER JOIN inserted i ON a.AssetId = i.AssetId;
                END;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Asset_GenerateAssetCode')
                    DROP TRIGGER TR_Asset_GenerateAssetCode;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Asset_UpdateModifiedDate')
                    DROP TRIGGER TR_Asset_UpdateModifiedDate;
                GO
            ");
        }
    }
}