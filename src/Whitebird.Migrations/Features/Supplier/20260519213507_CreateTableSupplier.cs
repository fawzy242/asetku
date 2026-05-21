using FluentMigrator;

namespace Whitebird.Migrations.Features.Supplier
{
    [Migration(20260519213507)]
    public class CreateTableSupplier : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Supplier')
        BEGIN
            CREATE TABLE [dbo].[Supplier] (
                [SupplierId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_Supplier_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_Supplier_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Supplier_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [SupplierName] NVARCHAR(100) NULL,
                [ContactPerson] NVARCHAR(100) NULL,
                [PhoneNumber] NVARCHAR(20) NULL,
                [Email] NVARCHAR(100) NULL,
                [Address] NVARCHAR(500) NULL,
                CONSTRAINT [PK_Supplier] PRIMARY KEY ([SupplierId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES (menggunakan OBJECT_ID yang lebih aman)
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Supplier_SupplierName' AND object_id = OBJECT_ID('Supplier'))
            CREATE INDEX [IX_Supplier_SupplierName]
            ON [dbo].[Supplier]([SupplierName]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Supplier_Email' AND object_id = OBJECT_ID('Supplier'))
            CREATE INDEX [IX_Supplier_Email]
            ON [dbo].[Supplier]([Email]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Supplier_IsActive' AND object_id = OBJECT_ID('Supplier'))
            CREATE INDEX [IX_Supplier_IsActive]
            ON [dbo].[Supplier]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Supplier_IsActive' AND object_id = OBJECT_ID('Supplier'))
            DROP INDEX [IX_Supplier_IsActive] ON [dbo].[Supplier];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Supplier_Email' AND object_id = OBJECT_ID('Supplier'))
            DROP INDEX [IX_Supplier_Email] ON [dbo].[Supplier];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Supplier_SupplierName' AND object_id = OBJECT_ID('Supplier'))
            DROP INDEX [IX_Supplier_SupplierName] ON [dbo].[Supplier];

        -- ============================================================
        -- DROP TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Supplier')
            DROP TABLE [dbo].[Supplier];
    ");
        }
    }
}