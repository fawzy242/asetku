using FluentMigrator;

namespace Whitebird.Migrations.Features.Asset
{
    [Migration(20260519213608)]
    public class CreateTableAsset : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Asset')
        BEGIN
            CREATE TABLE [dbo].[Asset] (
                [AssetId] INT IDENTITY(1,1) NOT NULL,
                [Notes] NVARCHAR(500) NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_Asset_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_Asset_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Asset_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [AssetCode] NVARCHAR(50) NULL,
                [AssetName] NVARCHAR(100) NULL,
                [CategoryId] INT NOT NULL,
                [SupplierId] INT NULL,
                [OfficeId] INT NULL,
                [Brand] NVARCHAR(50) NULL,
                [Model] NVARCHAR(50) NULL,
                [SerialNumber] NVARCHAR(50) NULL,
                [Imei] NVARCHAR(50) NULL,
                [Hostname] NVARCHAR(50) NULL,
                [IpAddress] NVARCHAR(50) NULL,
                [MacAddress] NVARCHAR(50) NULL,
                [PurchaseDate] DATE NULL,
                [PurchasePrice] DECIMAL(18,2) NULL,
                [InvoiceNumber] NVARCHAR(50) NULL,
                [WarrantyPeriod] INT NULL,
                [WarrantyExpiryDate] DATE NULL,
                [AssetConditionPurchase] INT NULL,
                [AssetCondition] INT NULL,
                [ResidualValue] DECIMAL(18,2) NULL,
                [UsefulLife] INT NULL,
                [DepreciationStartDate] DATE NULL,
                [LastMaintenanceDate] DATE NULL,
                [NextMaintenanceDate] DATE NULL,
                [OperasionalOffice] BIT NULL,
                CONSTRAINT [PK_Asset] PRIMARY KEY ([AssetId]),
                CONSTRAINT [UQ_Asset_AssetCode] UNIQUE ([AssetCode]),
                CONSTRAINT [FK_Asset_Category]
                    FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Category]([CategoryId]),
                CONSTRAINT [FK_Asset_Supplier]
                    FOREIGN KEY ([SupplierId]) REFERENCES [dbo].[Supplier]([SupplierId]),
                CONSTRAINT [FK_Asset_Office]
                    FOREIGN KEY ([OfficeId]) REFERENCES [dbo].[Office]([OfficeId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES (menggunakan OBJECT_ID yang lebih aman)
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_AssetName' AND object_id = OBJECT_ID('Asset'))
            CREATE INDEX [IX_Asset_AssetName]
            ON [dbo].[Asset]([AssetName]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_CategoryId' AND object_id = OBJECT_ID('Asset'))
            CREATE INDEX [IX_Asset_CategoryId]
            ON [dbo].[Asset]([CategoryId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_SupplierId' AND object_id = OBJECT_ID('Asset'))
            CREATE INDEX [IX_Asset_SupplierId]
            ON [dbo].[Asset]([SupplierId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_OfficeId' AND object_id = OBJECT_ID('Asset'))
            CREATE INDEX [IX_Asset_OfficeId]
            ON [dbo].[Asset]([OfficeId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_SerialNumber' AND object_id = OBJECT_ID('Asset'))
            CREATE INDEX [IX_Asset_SerialNumber]
            ON [dbo].[Asset]([SerialNumber])
            WHERE [SerialNumber] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_AssetCondition' AND object_id = OBJECT_ID('Asset'))
            CREATE INDEX [IX_Asset_AssetCondition]
            ON [dbo].[Asset]([AssetCondition]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_PurchaseDate' AND object_id = OBJECT_ID('Asset'))
            CREATE INDEX [IX_Asset_PurchaseDate]
            ON [dbo].[Asset]([PurchaseDate] DESC);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_WarrantyExpiryDate' AND object_id = OBJECT_ID('Asset'))
            CREATE INDEX [IX_Asset_WarrantyExpiryDate]
            ON [dbo].[Asset]([WarrantyExpiryDate]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_NextMaintenanceDate' AND object_id = OBJECT_ID('Asset'))
            CREATE INDEX [IX_Asset_NextMaintenanceDate]
            ON [dbo].[Asset]([NextMaintenanceDate]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_IsActive' AND object_id = OBJECT_ID('Asset'))
            CREATE INDEX [IX_Asset_IsActive]
            ON [dbo].[Asset]([IsActive])
            WHERE [IsActive] = 1;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_OperasionalOffice' AND object_id = OBJECT_ID('Asset'))
            CREATE INDEX [IX_Asset_OperasionalOffice]
            ON [dbo].[Asset]([OperasionalOffice], [IsActive])
            WHERE [OperasionalOffice] = 1 AND [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_OperasionalOffice' AND object_id = OBJECT_ID('Asset'))
            DROP INDEX [IX_Asset_OperasionalOffice] ON [dbo].[Asset];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_IsActive' AND object_id = OBJECT_ID('Asset'))
            DROP INDEX [IX_Asset_IsActive] ON [dbo].[Asset];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_NextMaintenanceDate' AND object_id = OBJECT_ID('Asset'))
            DROP INDEX [IX_Asset_NextMaintenanceDate] ON [dbo].[Asset];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_WarrantyExpiryDate' AND object_id = OBJECT_ID('Asset'))
            DROP INDEX [IX_Asset_WarrantyExpiryDate] ON [dbo].[Asset];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_PurchaseDate' AND object_id = OBJECT_ID('Asset'))
            DROP INDEX [IX_Asset_PurchaseDate] ON [dbo].[Asset];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_AssetCondition' AND object_id = OBJECT_ID('Asset'))
            DROP INDEX [IX_Asset_AssetCondition] ON [dbo].[Asset];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_SerialNumber' AND object_id = OBJECT_ID('Asset'))
            DROP INDEX [IX_Asset_SerialNumber] ON [dbo].[Asset];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_OfficeId' AND object_id = OBJECT_ID('Asset'))
            DROP INDEX [IX_Asset_OfficeId] ON [dbo].[Asset];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_SupplierId' AND object_id = OBJECT_ID('Asset'))
            DROP INDEX [IX_Asset_SupplierId] ON [dbo].[Asset];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_CategoryId' AND object_id = OBJECT_ID('Asset'))
            DROP INDEX [IX_Asset_CategoryId] ON [dbo].[Asset];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asset_AssetName' AND object_id = OBJECT_ID('Asset'))
            DROP INDEX [IX_Asset_AssetName] ON [dbo].[Asset];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Asset')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('Asset') AND name = 'FK_Asset_Office')
                ALTER TABLE [dbo].[Asset] DROP CONSTRAINT [FK_Asset_Office];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('Asset') AND name = 'FK_Asset_Supplier')
                ALTER TABLE [dbo].[Asset] DROP CONSTRAINT [FK_Asset_Supplier];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('Asset') AND name = 'FK_Asset_Category')
                ALTER TABLE [dbo].[Asset] DROP CONSTRAINT [FK_Asset_Category];
            
            DROP TABLE [dbo].[Asset];
        END
    ");
        }
    }
}