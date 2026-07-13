using FluentMigrator;

namespace Whitebird.Migrations.Features.Asset
{
    [Migration(20260713205330)]
    public class CreateTableSoftwareInstallation : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SoftwareInstallation')
        BEGIN
            CREATE TABLE [dbo].[SoftwareInstallation] (
                [SoftwareInstallationId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_SoftwareInstallation_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_SoftwareInstallation_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_SoftwareInstallation_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [AssetId] INT NOT NULL,
                [UserPassword] NVARCHAR(50) NULL,
                [AdminOperationalPassword] NVARCHAR(50) NULL,
                [OperatingSystem] INT NULL,
                [MicrosoftOffice] INT NULL,
                [VisualStudio] INT NULL,
                [DBMS] INT NULL,
                [ConnectToNAS] BIT NOT NULL CONSTRAINT [DF_SoftwareInstallation_ConnectToNAS] DEFAULT 0,
                CONSTRAINT [PK_SoftwareInstallation] PRIMARY KEY ([SoftwareInstallationId]),
                CONSTRAINT [FK_SoftwareInstallation_Asset]
                    FOREIGN KEY ([AssetId]) REFERENCES [dbo].[Asset]([AssetId]),
                CONSTRAINT [UQ_SoftwareInstallation_AssetId] UNIQUE ([AssetId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SoftwareInstallation_OperatingSystem' AND object_id = OBJECT_ID('SoftwareInstallation'))
            CREATE INDEX [IX_SoftwareInstallation_OperatingSystem]
            ON [dbo].[SoftwareInstallation]([OperatingSystem])
            WHERE [OperatingSystem] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SoftwareInstallation_MicrosoftOffice' AND object_id = OBJECT_ID('SoftwareInstallation'))
            CREATE INDEX [IX_SoftwareInstallation_MicrosoftOffice]
            ON [dbo].[SoftwareInstallation]([MicrosoftOffice])
            WHERE [MicrosoftOffice] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SoftwareInstallation_VisualStudio' AND object_id = OBJECT_ID('SoftwareInstallation'))
            CREATE INDEX [IX_SoftwareInstallation_VisualStudio]
            ON [dbo].[SoftwareInstallation]([VisualStudio])
            WHERE [VisualStudio] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SoftwareInstallation_DBMS' AND object_id = OBJECT_ID('SoftwareInstallation'))
            CREATE INDEX [IX_SoftwareInstallation_DBMS]
            ON [dbo].[SoftwareInstallation]([DBMS])
            WHERE [DBMS] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SoftwareInstallation_IsActive' AND object_id = OBJECT_ID('SoftwareInstallation'))
            CREATE INDEX [IX_SoftwareInstallation_IsActive]
            ON [dbo].[SoftwareInstallation]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SoftwareInstallation_IsActive' AND object_id = OBJECT_ID('SoftwareInstallation'))
            DROP INDEX [IX_SoftwareInstallation_IsActive] ON [dbo].[SoftwareInstallation];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SoftwareInstallation_DBMS' AND object_id = OBJECT_ID('SoftwareInstallation'))
            DROP INDEX [IX_SoftwareInstallation_DBMS] ON [dbo].[SoftwareInstallation];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SoftwareInstallation_VisualStudio' AND object_id = OBJECT_ID('SoftwareInstallation'))
            DROP INDEX [IX_SoftwareInstallation_VisualStudio] ON [dbo].[SoftwareInstallation];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SoftwareInstallation_MicrosoftOffice' AND object_id = OBJECT_ID('SoftwareInstallation'))
            DROP INDEX [IX_SoftwareInstallation_MicrosoftOffice] ON [dbo].[SoftwareInstallation];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SoftwareInstallation_OperatingSystem' AND object_id = OBJECT_ID('SoftwareInstallation'))
            DROP INDEX [IX_SoftwareInstallation_OperatingSystem] ON [dbo].[SoftwareInstallation];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SoftwareInstallation')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('SoftwareInstallation') AND name = 'FK_SoftwareInstallation_Asset')
                ALTER TABLE [dbo].[SoftwareInstallation] DROP CONSTRAINT [FK_SoftwareInstallation_Asset];
            
            DROP TABLE [dbo].[SoftwareInstallation];
        END
    ");
        }
    }
}