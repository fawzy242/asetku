using FluentMigrator;

namespace Whitebird.Migrations.Features.MasterData
{
    [Migration(20260519213759)]
    public class SeedMasterData : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM [dbo].[MasterData])
        BEGIN
            INSERT INTO [dbo].[MasterData] ([ReferenceCode], [ReferenceName], [MasterDataName])
            VALUES
                /* POSITION */
                (1, 'Position', 'Director'),
                (2, 'Position', 'Vice President'),
                (3, 'Position', 'Head Of Department'),
                (4, 'Position', 'Manager'),
                (5, 'Position', 'Supervisor'),
                (6, 'Position', 'Senior Associate'),
                (7, 'Position', 'Junior Associate'),

                /* EMPLOYEE STATUS */
                (1, 'EmployeeStatus', 'Internship'),
                (2, 'EmployeeStatus', 'Training'),
                (3, 'EmployeeStatus', 'Contract'),
                (4, 'EmployeeStatus', 'Permanent Employee'),
                (5, 'EmployeeStatus', 'Resigned'),

                /* OFFICE TYPE */
                (1, 'OfficeType', 'Head Office'),
                (2, 'OfficeType', 'Branch Office'),

                /* ASSET CONDITION PURCHASE */
                (1, 'AssetConditionPurchase', 'New'),
                (2, 'AssetConditionPurchase', 'Second Hand'),

                /* ASSET CONDITION */
                (1, 'AssetCondition', 'Good'),
                (2, 'AssetCondition', 'Normal'),
                (3, 'AssetCondition', 'Damaged'),

                /* TRANSACTION TYPE */
                (1, 'TransactionType', 'HANDOVER'),
                (2, 'TransactionType', 'TRANSFER'),
                (3, 'TransactionType', 'LOAN'),
                (4, 'TransactionType', 'RETURN'),
                (5, 'TransactionType', 'LOAN RETURN'),
                (6, 'TransactionType', 'MAINTENANCE'),
                (7, 'TransactionType', 'POST MAINTENANCE'),
                (8, 'TransactionType', 'DISPOSAL'),

                /* MAINTENANCE TYPE */
                (1, 'MaintenanceType', 'PREVENTIVE MAINTENANCE'),
                (2, 'MaintenanceType', 'CORRECTIVE MAINTENANCE'),
                (3, 'MaintenanceType', 'EMERGENCY REPAIR'),
                (4, 'MaintenanceType', 'HARDWARE REPLACEMENT'),
                (5, 'MaintenanceType', 'SOFTWARE UPDATE'),
                (6, 'MaintenanceType', 'INSPECTION'),
                (7, 'MaintenanceType', 'CLEANING'),
                (8, 'MaintenanceType', 'CALIBRATION');
        END;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"");
        }
    }
}
