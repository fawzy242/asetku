using FluentMigrator;

namespace Whitebird.Migrations.Features.AssetTransaction
{
    [Migration(20260519213629)]
    public class CreateTableAssetTransaction : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AssetTransaction')
        BEGIN
            CREATE TABLE [dbo].[AssetTransaction] (
                [AssetTransactionId] INT IDENTITY(1,1) NOT NULL,
                [Notes] NVARCHAR(500) NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_AssetTransaction_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_AssetTransaction_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_AssetTransaction_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [Approved] BIT NULL,
                [ApprovedBy] NVARCHAR(50) NULL,
                [Completed] BIT NULL,
                [CompletedBy] NVARCHAR(50) NULL,
                [AssetTransactionCode] NVARCHAR(50) NULL,
                [TransactionDate] DATETIME NOT NULL,
                [TransactionType] INT NOT NULL,
                [AssetId] INT NOT NULL,
                [EmployeeId] INT NULL,
                [OfficeId] INT NULL,
                [Condition] INT NULL,
                [FromAssetTransactionId] INT NULL,
                [ExpectedLoanReturnDate] DATE NULL,
                [MaintenanceEmployeeId] INT NULL,
                [MaintenanceType] INT NULL,
                [MaintenanceCost] DECIMAL(18,2) NULL,
                CONSTRAINT [PK_AssetTransaction] PRIMARY KEY ([AssetTransactionId]),
                CONSTRAINT [UQ_AssetTransaction_AssetTransactionCode] UNIQUE ([AssetTransactionCode]),
                CONSTRAINT [FK_AssetTransaction_Asset]
                    FOREIGN KEY ([AssetId]) REFERENCES [dbo].[Asset]([AssetId]),
                CONSTRAINT [FK_AssetTransaction_PreviousTransaction]
                    FOREIGN KEY ([FromAssetTransactionId]) REFERENCES [dbo].[AssetTransaction]([AssetTransactionId]),
                CONSTRAINT [FK_AssetTransaction_Employee]
                    FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employee]([EmployeeId]),
                CONSTRAINT [FK_AssetTransaction_Office]
                    FOREIGN KEY ([OfficeId]) REFERENCES [dbo].[Office]([OfficeId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_AssetId' AND object_id = OBJECT_ID('AssetTransaction'))
            CREATE INDEX [IX_AssetTransaction_AssetId]
            ON [dbo].[AssetTransaction]([AssetId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_TransactionDate' AND object_id = OBJECT_ID('AssetTransaction'))
            CREATE INDEX [IX_AssetTransaction_TransactionDate]
            ON [dbo].[AssetTransaction]([TransactionDate] DESC);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_TransactionType' AND object_id = OBJECT_ID('AssetTransaction'))
            CREATE INDEX [IX_AssetTransaction_TransactionType]
            ON [dbo].[AssetTransaction]([TransactionType]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_EmployeeId' AND object_id = OBJECT_ID('AssetTransaction'))
            CREATE INDEX [IX_AssetTransaction_EmployeeId]
            ON [dbo].[AssetTransaction]([EmployeeId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_OfficeId' AND object_id = OBJECT_ID('AssetTransaction'))
            CREATE INDEX [IX_AssetTransaction_OfficeId]
            ON [dbo].[AssetTransaction]([OfficeId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_Approved' AND object_id = OBJECT_ID('AssetTransaction'))
            CREATE INDEX [IX_AssetTransaction_Approved]
            ON [dbo].[AssetTransaction]([Approved])
            WHERE [Approved] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_Completed' AND object_id = OBJECT_ID('AssetTransaction'))
            CREATE INDEX [IX_AssetTransaction_Completed]
            ON [dbo].[AssetTransaction]([Completed])
            WHERE [Completed] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_ExpectedLoanReturnDate' AND object_id = OBJECT_ID('AssetTransaction'))
            CREATE INDEX [IX_AssetTransaction_ExpectedLoanReturnDate]
            ON [dbo].[AssetTransaction]([ExpectedLoanReturnDate])
            WHERE [ExpectedLoanReturnDate] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_FromAssetTransactionId' AND object_id = OBJECT_ID('AssetTransaction'))
            CREATE INDEX [IX_AssetTransaction_FromAssetTransactionId]
            ON [dbo].[AssetTransaction]([FromAssetTransactionId])
            WHERE [FromAssetTransactionId] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_IsActive' AND object_id = OBJECT_ID('AssetTransaction'))
            CREATE INDEX [IX_AssetTransaction_IsActive]
            ON [dbo].[AssetTransaction]([IsActive])
            WHERE [IsActive] = 1;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_AssetId_TransactionDate' AND object_id = OBJECT_ID('AssetTransaction'))
            CREATE INDEX [IX_AssetTransaction_AssetId_TransactionDate]
            ON [dbo].[AssetTransaction]([AssetId], [TransactionDate] DESC);
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_AssetId_TransactionDate' AND object_id = OBJECT_ID('AssetTransaction'))
            DROP INDEX [IX_AssetTransaction_AssetId_TransactionDate] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_IsActive' AND object_id = OBJECT_ID('AssetTransaction'))
            DROP INDEX [IX_AssetTransaction_IsActive] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_FromAssetTransactionId' AND object_id = OBJECT_ID('AssetTransaction'))
            DROP INDEX [IX_AssetTransaction_FromAssetTransactionId] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_ExpectedLoanReturnDate' AND object_id = OBJECT_ID('AssetTransaction'))
            DROP INDEX [IX_AssetTransaction_ExpectedLoanReturnDate] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_Approved' AND object_id = OBJECT_ID('AssetTransaction'))
            DROP INDEX [IX_AssetTransaction_Approved] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_Completed' AND object_id = OBJECT_ID('AssetTransaction'))
            DROP INDEX [IX_AssetTransaction_Completed] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_OfficeId' AND object_id = OBJECT_ID('AssetTransaction'))
            DROP INDEX [IX_AssetTransaction_OfficeId] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_EmployeeId' AND object_id = OBJECT_ID('AssetTransaction'))
            DROP INDEX [IX_AssetTransaction_EmployeeId] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_TransactionType' AND object_id = OBJECT_ID('AssetTransaction'))
            DROP INDEX [IX_AssetTransaction_TransactionType] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_TransactionDate' AND object_id = OBJECT_ID('AssetTransaction'))
            DROP INDEX [IX_AssetTransaction_TransactionDate] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AssetTransaction_AssetId' AND object_id = OBJECT_ID('AssetTransaction'))
            DROP INDEX [IX_AssetTransaction_AssetId] ON [dbo].[AssetTransaction];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AssetTransaction')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AssetTransaction') AND name = 'FK_AssetTransaction_Office')
                ALTER TABLE [dbo].[AssetTransaction] DROP CONSTRAINT [FK_AssetTransaction_Office];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AssetTransaction') AND name = 'FK_AssetTransaction_Employee')
                ALTER TABLE [dbo].[AssetTransaction] DROP CONSTRAINT [FK_AssetTransaction_Employee];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AssetTransaction') AND name = 'FK_AssetTransaction_PreviousTransaction')
                ALTER TABLE [dbo].[AssetTransaction] DROP CONSTRAINT [FK_AssetTransaction_PreviousTransaction];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('AssetTransaction') AND name = 'FK_AssetTransaction_Asset')
                ALTER TABLE [dbo].[AssetTransaction] DROP CONSTRAINT [FK_AssetTransaction_Asset];
            
            DROP TABLE [dbo].[AssetTransaction];
        END
    ");
        }
    }
}