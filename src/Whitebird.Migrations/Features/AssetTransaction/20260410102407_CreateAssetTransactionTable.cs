using FluentMigrator;

namespace Whitebird.Migrations.Features.AssetTransaction
{
    [Migration(20260410102407)]
    public class CreateAssetTransactionTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'AssetTransaction' AND xtype = 'U')
                BEGIN
                    CREATE TABLE AssetTransaction (
                        AssetTransactionId INT NOT NULL CONSTRAINT DF_AssetTransaction_AssetTransactionId DEFAULT NEXT VALUE FOR Seq_AssetTransactionId,
                        AssetId INT NOT NULL,
                        TransactionType NVARCHAR(50) NOT NULL,
                        FromEmployeeId INT NULL,
                        ToEmployeeId INT NULL,
                        FromLocationId INT NULL,
                        ToLocationId INT NULL,
                        TransactionDate DATETIME NOT NULL,
                        ExpectedReturnDate DATETIME NULL,
                        ActualReturnDate DATETIME NULL,
                        Notes NVARCHAR(500) NULL,
                        ConditionBefore NVARCHAR(20) NULL,
                        ConditionAfter NVARCHAR(20) NULL,
                        TransactionStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_AssetTransaction_TransactionStatus DEFAULT 'Pending',
                        ApprovedBy INT NULL,
                        MaintenanceType NVARCHAR(50) NULL,
                        MaintenanceCost DECIMAL(18,2) NULL,
                        VendorName NVARCHAR(100) NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_AssetTransaction_IsActive DEFAULT 1,
                        CreatedDate DATETIME NOT NULL CONSTRAINT DF_AssetTransaction_CreatedDate DEFAULT GETDATE(),
                        CreatedBy NVARCHAR(50) NOT NULL CONSTRAINT DF_AssetTransaction_CreatedBy DEFAULT 'System',
                        ModifiedDate DATETIME NULL,
                        ModifiedBy NVARCHAR(50) NULL,
                        
                        CONSTRAINT PK_AssetTransaction PRIMARY KEY (AssetTransactionId),
                        CONSTRAINT FK_AssetTransaction_Asset FOREIGN KEY (AssetId) REFERENCES Asset(AssetId),
                        CONSTRAINT FK_AssetTransaction_FromEmployee FOREIGN KEY (FromEmployeeId) REFERENCES Employee(EmployeeId),
                        CONSTRAINT FK_AssetTransaction_ToEmployee FOREIGN KEY (ToEmployeeId) REFERENCES Employee(EmployeeId),
                        CONSTRAINT FK_AssetTransaction_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES Employee(EmployeeId)
                    );
                END;
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AssetTransaction_AssetId' AND object_id = OBJECT_ID('AssetTransaction'))
                    CREATE INDEX IX_AssetTransaction_AssetId ON AssetTransaction(AssetId);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AssetTransaction_TransactionDate' AND object_id = OBJECT_ID('AssetTransaction'))
                    CREATE INDEX IX_AssetTransaction_TransactionDate ON AssetTransaction(TransactionDate DESC);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AssetTransaction_TransactionType' AND object_id = OBJECT_ID('AssetTransaction'))
                    CREATE INDEX IX_AssetTransaction_TransactionType ON AssetTransaction(TransactionType);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AssetTransaction_TransactionStatus' AND object_id = OBJECT_ID('AssetTransaction'))
                    CREATE INDEX IX_AssetTransaction_TransactionStatus ON AssetTransaction(TransactionStatus);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AssetTransaction_FromEmployeeId' AND object_id = OBJECT_ID('AssetTransaction'))
                    CREATE INDEX IX_AssetTransaction_FromEmployeeId ON AssetTransaction(FromEmployeeId);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AssetTransaction_ToEmployeeId' AND object_id = OBJECT_ID('AssetTransaction'))
                    CREATE INDEX IX_AssetTransaction_ToEmployeeId ON AssetTransaction(ToEmployeeId);
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name = 'AssetTransaction' AND xtype = 'U')
                    DROP TABLE AssetTransaction;
                GO
            ");
        }
    }
}
