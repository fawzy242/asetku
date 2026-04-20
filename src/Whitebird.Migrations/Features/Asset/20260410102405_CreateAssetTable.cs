using FluentMigrator;

namespace Whitebird.Migrations.Features.Asset
{
    [Migration(20260410102405)]
    public class CreateAssetTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Asset' AND xtype = 'U')
                BEGIN
                    CREATE TABLE Asset (
                        AssetId INT NOT NULL CONSTRAINT DF_Asset_AssetId DEFAULT NEXT VALUE FOR Seq_AssetId,
                        AssetCode NVARCHAR(50) NOT NULL UNIQUE,
                        AssetName NVARCHAR(100) NOT NULL,
                        CategoryId INT NOT NULL,
                        SubCategory NVARCHAR(50) NULL,
                        AssetType NVARCHAR(50) NULL,
                        Brand NVARCHAR(50) NULL,
                        Model NVARCHAR(50) NULL,
                        SerialNumber NVARCHAR(50) NULL,
                        Imei NVARCHAR(50) NULL,
                        MacAddress NVARCHAR(50) NULL,
                        PurchaseDate DATETIME NULL,
                        PurchasePrice DECIMAL(18,2) NULL,
                        InvoiceNumber NVARCHAR(50) NULL,
                        SupplierId INT NULL,
                        WarrantyPeriod INT NULL,
                        WarrantyExpiryDate DATETIME NULL,
                        Condition NVARCHAR(20) NULL,
                        Status NVARCHAR(20) NOT NULL CONSTRAINT DF_Asset_Status DEFAULT 'Available',
                        Location NVARCHAR(100) NULL,
                        CurrentHolderId INT NULL,
                        ResponsiblePartyId INT NULL,
                        ResidualValue DECIMAL(18,2) NULL,
                        UsefulLife INT NULL,
                        DepreciationStartDate DATETIME NULL,
                        Notes NVARCHAR(500) NULL,
                        LastMaintenanceDate DATETIME NULL,
                        NextMaintenanceDate DATETIME NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_Asset_IsActive DEFAULT 1,
                        CreatedDate DATETIME NOT NULL CONSTRAINT DF_Asset_CreatedDate DEFAULT GETDATE(),
                        CreatedBy NVARCHAR(50) NOT NULL CONSTRAINT DF_Asset_CreatedBy DEFAULT 'System',
                        ModifiedDate DATETIME NULL,
                        ModifiedBy NVARCHAR(50) NULL,
                        
                        CONSTRAINT PK_Asset PRIMARY KEY (AssetId),
                        CONSTRAINT FK_Asset_Category FOREIGN KEY (CategoryId) REFERENCES Category(CategoryId),
                        CONSTRAINT FK_Asset_Supplier FOREIGN KEY (SupplierId) REFERENCES Supplier(SupplierId),
                        CONSTRAINT FK_Asset_CurrentHolder FOREIGN KEY (CurrentHolderId) REFERENCES Employee(EmployeeId),
                        CONSTRAINT FK_Asset_ResponsibleParty FOREIGN KEY (ResponsiblePartyId) REFERENCES Employee(EmployeeId)
                    );
                END;
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_AssetCode' AND object_id = OBJECT_ID('Asset'))
                    CREATE INDEX IX_Asset_AssetCode ON Asset(AssetCode);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_Status' AND object_id = OBJECT_ID('Asset'))
                    CREATE INDEX IX_Asset_Status ON Asset(Status);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_CategoryId' AND object_id = OBJECT_ID('Asset'))
                    CREATE INDEX IX_Asset_CategoryId ON Asset(CategoryId);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_CurrentHolderId' AND object_id = OBJECT_ID('Asset'))
                    CREATE INDEX IX_Asset_CurrentHolderId ON Asset(CurrentHolderId);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_SerialNumber' AND object_id = OBJECT_ID('Asset'))
                    CREATE INDEX IX_Asset_SerialNumber ON Asset(SerialNumber);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_IsActive' AND object_id = OBJECT_ID('Asset'))
                    CREATE INDEX IX_Asset_IsActive ON Asset(IsActive);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Asset_SupplierId' AND object_id = OBJECT_ID('Asset'))
                    CREATE INDEX IX_Asset_SupplierId ON Asset(SupplierId);
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name = 'Asset' AND xtype = 'U')
                    DROP TABLE Asset;
                GO
            ");
        }
    }
}
