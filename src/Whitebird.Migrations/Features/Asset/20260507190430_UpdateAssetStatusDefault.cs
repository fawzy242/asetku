using FluentMigrator;

namespace Whitebird.Migrations.Features.Asset
{
    [Migration(20260507190430)]
    public class UpdateAssetStatusDefault : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Drop old constraint if exists from previous migration
                IF EXISTS (SELECT * FROM sys.default_constraints WHERE name = 'DF_Asset_Status')
                    ALTER TABLE Asset DROP CONSTRAINT DF_Asset_Status;
                GO

                -- Recreate dengan default yang sama
                ALTER TABLE Asset
                ADD CONSTRAINT DF_Asset_Status DEFAULT 'Available' FOR Status;
                GO

                -- Add check constraint untuk memastikan status valid
                -- Support skenario baru: On Loan, In Maintenance, Damaged
                IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Asset_Status')
                BEGIN
                    ALTER TABLE Asset
                    ADD CONSTRAINT CK_Asset_Status
                    CHECK (Status IN (
                        'Available', 
                        'Assigned', 
                        'On Loan', 
                        'In Maintenance', 
                        'Under Repair', 
                        'Damaged', 
                        'Retired', 
                        'Disposed'
                    ));
                END;
                GO

                -- Index untuk asset status dengan filter
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_Status_Active' AND object_id = OBJECT_ID('Asset'))
                    CREATE INDEX IX_Asset_Status_Active ON Asset(Status) WHERE IsActive = 1;
                GO

                -- Index untuk NextMaintenanceDate (support dashboard upcoming maintenance)
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_NextMaintenanceDate' AND object_id = OBJECT_ID('Asset'))
                    CREATE INDEX IX_Asset_NextMaintenanceDate ON Asset(NextMaintenanceDate)
                    WHERE NextMaintenanceDate IS NOT NULL AND IsActive = 1;
                GO

                -- Index untuk WarrantyExpiryDate (support dashboard expired warranty)
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_WarrantyExpiryDate' AND object_id = OBJECT_ID('Asset'))
                    CREATE INDEX IX_Asset_WarrantyExpiryDate ON Asset(WarrantyExpiryDate)
                    WHERE WarrantyExpiryDate IS NOT NULL AND IsActive = 1;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_WarrantyExpiryDate' AND object_id = OBJECT_ID('Asset'))
                    DROP INDEX IX_Asset_WarrantyExpiryDate ON Asset;
                GO

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_NextMaintenanceDate' AND object_id = OBJECT_ID('Asset'))
                    DROP INDEX IX_Asset_NextMaintenanceDate ON Asset;
                GO

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_Status_Active' AND object_id = OBJECT_ID('Asset'))
                    DROP INDEX IX_Asset_Status_Active ON Asset;
                GO

                IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Asset_Status')
                    ALTER TABLE Asset DROP CONSTRAINT CK_Asset_Status;
                GO
            ");
        }
    }
}
