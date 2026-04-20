using FluentMigrator;

namespace Whitebird.Migrations.Features.Supplier
{
    [Migration(20260410102359)]
    public class CreateSupplierTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Supplier' AND xtype = 'U')
                BEGIN
                    CREATE TABLE Supplier (
                        SupplierId INT NOT NULL CONSTRAINT DF_Supplier_SupplierId DEFAULT NEXT VALUE FOR Seq_SupplierId,
                        SupplierName NVARCHAR(100) NOT NULL,
                        ContactPerson NVARCHAR(100) NULL,
                        PhoneNumber NVARCHAR(20) NULL,
                        Email NVARCHAR(100) NULL,
                        Address NVARCHAR(500) NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_Supplier_IsActive DEFAULT 1,
                        CreatedDate DATETIME NOT NULL CONSTRAINT DF_Supplier_CreatedDate DEFAULT GETDATE(),
                        CreatedBy NVARCHAR(50) NOT NULL CONSTRAINT DF_Supplier_CreatedBy DEFAULT 'System',
                        ModifiedDate DATETIME NULL,
                        ModifiedBy NVARCHAR(50) NULL,
                        
                        CONSTRAINT PK_Supplier PRIMARY KEY (SupplierId)
                    );
                END;
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Supplier_SupplierName' AND object_id = OBJECT_ID('Supplier'))
                    CREATE INDEX IX_Supplier_SupplierName ON Supplier(SupplierName);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Supplier_IsActive' AND object_id = OBJECT_ID('Supplier'))
                    CREATE INDEX IX_Supplier_IsActive ON Supplier(IsActive);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Supplier_Email' AND object_id = OBJECT_ID('Supplier'))
                    CREATE INDEX IX_Supplier_Email ON Supplier(Email);
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name = 'Supplier' AND xtype = 'U')
                    DROP TABLE Supplier;
                GO
            ");
        }
    }
}
