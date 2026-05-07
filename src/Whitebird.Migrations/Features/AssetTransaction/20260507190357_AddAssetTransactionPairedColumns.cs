using FluentMigrator;

namespace Whitebird.Migrations.Features.AssetTransaction
{
    [Migration(20260507190357)]
    public class AddAssetTransactionPairedColumns : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Add PairedTransactionId untuk pairing: LOAN↔LOAN_RETURN, MAINTENANCE↔POST_MAINTENANCE, HANDOVER↔RETURN
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'PairedTransactionId' AND object_id = OBJECT_ID('AssetTransaction'))
                BEGIN
                    ALTER TABLE AssetTransaction
                    ADD PairedTransactionId INT NULL;
                END;
                GO

                -- Add DamageReason untuk mencatat alasan kerusakan saat RETURN
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE name = 'DamageReason' AND object_id = OBJECT_ID('AssetTransaction'))
                BEGIN
                    ALTER TABLE AssetTransaction
                    ADD DamageReason NVARCHAR(100) NULL;
                END;
                GO

                -- Add FK constraint untuk PairedTransactionId
                IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AssetTransaction_Paired')
                BEGIN
                    ALTER TABLE AssetTransaction
                    ADD CONSTRAINT FK_AssetTransaction_Paired
                        FOREIGN KEY (PairedTransactionId)
                        REFERENCES AssetTransaction(AssetTransactionId);
                END;
                GO

                -- Add index untuk lookup paired transaction
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AssetTransaction_PairedTransactionId' AND object_id = OBJECT_ID('AssetTransaction'))
                    CREATE INDEX IX_AssetTransaction_PairedTransactionId ON AssetTransaction(PairedTransactionId);
                GO

                -- Add filtered index untuk tracking overdue loans
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AssetTransaction_ExpectedReturnDate' AND object_id = OBJECT_ID('AssetTransaction'))
                    CREATE INDEX IX_AssetTransaction_ExpectedReturnDate ON AssetTransaction(ExpectedReturnDate)
                    WHERE ExpectedReturnDate IS NOT NULL;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AssetTransaction_ExpectedReturnDate' AND object_id = OBJECT_ID('AssetTransaction'))
                    DROP INDEX IX_AssetTransaction_ExpectedReturnDate ON AssetTransaction;
                GO

                IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AssetTransaction_PairedTransactionId' AND object_id = OBJECT_ID('AssetTransaction'))
                    DROP INDEX IX_AssetTransaction_PairedTransactionId ON AssetTransaction;
                GO

                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AssetTransaction_Paired')
                    ALTER TABLE AssetTransaction DROP CONSTRAINT FK_AssetTransaction_Paired;
                GO

                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'DamageReason' AND object_id = OBJECT_ID('AssetTransaction'))
                    ALTER TABLE AssetTransaction DROP COLUMN DamageReason;
                GO

                IF EXISTS (SELECT * FROM sys.columns WHERE name = 'PairedTransactionId' AND object_id = OBJECT_ID('AssetTransaction'))
                    ALTER TABLE AssetTransaction DROP COLUMN PairedTransactionId;
                GO
            ");
        }
    }
}
