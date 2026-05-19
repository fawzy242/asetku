using FluentMigrator;

namespace Whitebird.Migrations.Features.AssetTransaction
{
    [Migration(20260519213629)]
    public class CreateTableAssetTransaction : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        IF NOT EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'AssetTransaction' AND [xtype] = 'U')
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
                [TransactionDate] DATETIME NOT NULL,
                [TransactionType] INT NOT NULL,
                [FromAssetTransactionId] INT NULL,
                [AssetId] INT NOT NULL,
                [FromEmployeeId] INT NULL,
                [ToEmployeeId] INT NULL,
                [ToLocationId] INT NULL,
                [ExpectedReturnDate] DATE NULL,
                [ActualReturnDate] DATE NULL,
                [ConditionBefore] INT NULL,
                [ConditionAfter] INT NULL,
                [MaintenanceType] INT NULL,
                [MaintenanceCost] DECIMAL(18,2) NULL,
                CONSTRAINT [PK_AssetTransaction] PRIMARY KEY ([AssetTransactionId]),
                CONSTRAINT [FK_AssetTransaction_Asset]
                    FOREIGN KEY ([AssetId]) REFERENCES [dbo].[Asset]([AssetId]),
                CONSTRAINT [FK_AssetTransaction_PreviousTransaction]
                    FOREIGN KEY ([FromAssetTransactionId]) REFERENCES [dbo].[AssetTransaction]([AssetTransactionId]),
                CONSTRAINT [FK_AssetTransaction_FromEmployee]
                    FOREIGN KEY ([FromEmployeeId]) REFERENCES [dbo].[Employee]([EmployeeId]),
                CONSTRAINT [FK_AssetTransaction_ToEmployee]
                    FOREIGN KEY ([ToEmployeeId]) REFERENCES [dbo].[Employee]([EmployeeId]),
                CONSTRAINT [FK_AssetTransaction_ToOffice]
                    FOREIGN KEY ([ToLocationId]) REFERENCES [dbo].[Office]([OfficeId])
            );
        END;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_AssetId')
            CREATE INDEX [IX_AssetTransaction_AssetId]
            ON [dbo].[AssetTransaction]([AssetId]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_TransactionDate')
            CREATE INDEX [IX_AssetTransaction_TransactionDate]
            ON [dbo].[AssetTransaction]([TransactionDate] DESC);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_TransactionType')
            CREATE INDEX [IX_AssetTransaction_TransactionType]
            ON [dbo].[AssetTransaction]([TransactionType]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_FromEmployeeId')
            CREATE INDEX [IX_AssetTransaction_FromEmployeeId]
            ON [dbo].[AssetTransaction]([FromEmployeeId]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_ToEmployeeId')
            CREATE INDEX [IX_AssetTransaction_ToEmployeeId]
            ON [dbo].[AssetTransaction]([ToEmployeeId]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_ToLocationId')
            CREATE INDEX [IX_AssetTransaction_ToLocationId]
            ON [dbo].[AssetTransaction]([ToLocationId]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_Approved')
            CREATE INDEX [IX_AssetTransaction_Approved]
            ON [dbo].[AssetTransaction]([Approved])
            WHERE [Approved] IS NOT NULL;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_ExpectedReturnDate')
            CREATE INDEX [IX_AssetTransaction_ExpectedReturnDate]
            ON [dbo].[AssetTransaction]([ExpectedReturnDate])
            WHERE [ExpectedReturnDate] IS NOT NULL;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_FromAssetTransactionId')
            CREATE INDEX [IX_AssetTransaction_FromAssetTransactionId]
            ON [dbo].[AssetTransaction]([FromAssetTransactionId])
            WHERE [FromAssetTransactionId] IS NOT NULL;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_IsActive')
            CREATE INDEX [IX_AssetTransaction_IsActive]
            ON [dbo].[AssetTransaction]([IsActive])
            WHERE [IsActive] = 1;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_AssetId_TransactionDate')
            CREATE INDEX [IX_AssetTransaction_AssetId_TransactionDate]
            ON [dbo].[AssetTransaction]([AssetId], [TransactionDate] DESC);
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_AssetId_TransactionDate')
            DROP INDEX [IX_AssetTransaction_AssetId_TransactionDate] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_IsActive')
            DROP INDEX [IX_AssetTransaction_IsActive] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_FromAssetTransactionId')
            DROP INDEX [IX_AssetTransaction_FromAssetTransactionId] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_ExpectedReturnDate')
            DROP INDEX [IX_AssetTransaction_ExpectedReturnDate] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_Approved')
            DROP INDEX [IX_AssetTransaction_Approved] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_ToLocationId')
            DROP INDEX [IX_AssetTransaction_ToLocationId] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_ToEmployeeId')
            DROP INDEX [IX_AssetTransaction_ToEmployeeId] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_FromEmployeeId')
            DROP INDEX [IX_AssetTransaction_FromEmployeeId] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_TransactionType')
            DROP INDEX [IX_AssetTransaction_TransactionType] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_TransactionDate')
            DROP INDEX [IX_AssetTransaction_TransactionDate] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_AssetTransaction_AssetId')
            DROP INDEX [IX_AssetTransaction_AssetId] ON [dbo].[AssetTransaction];

        IF EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'AssetTransaction' AND [xtype] = 'U')
        BEGIN
            ALTER TABLE [dbo].[AssetTransaction] DROP CONSTRAINT [FK_AssetTransaction_ToOffice];
            ALTER TABLE [dbo].[AssetTransaction] DROP CONSTRAINT [FK_AssetTransaction_ToEmployee];
            ALTER TABLE [dbo].[AssetTransaction] DROP CONSTRAINT [FK_AssetTransaction_FromEmployee];
            ALTER TABLE [dbo].[AssetTransaction] DROP CONSTRAINT [FK_AssetTransaction_PreviousTransaction];
            ALTER TABLE [dbo].[AssetTransaction] DROP CONSTRAINT [FK_AssetTransaction_Asset];
            DROP TABLE [dbo].[AssetTransaction];
        END;
    ");
        }
    }
}
