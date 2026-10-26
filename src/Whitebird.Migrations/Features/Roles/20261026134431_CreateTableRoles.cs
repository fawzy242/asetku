using FluentMigrator;

namespace Whitebird.Migrations.Features.Roles
{
    [Migration(20261026134431)]
    public class CreateTableRoles : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Roles')
        BEGIN
            CREATE TABLE [dbo].[Roles] (
                [RoleId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_Roles_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_Roles_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Roles_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [RoleCode] NVARCHAR(50) NULL,
                [RoleName] NVARCHAR(100) NOT NULL,
                [Description] NVARCHAR(500) NULL,
                CONSTRAINT [PK_Roles] PRIMARY KEY ([RoleId]),
                CONSTRAINT [UQ_Roles_RoleCode] UNIQUE ([RoleCode])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Roles_RoleName' AND object_id = OBJECT_ID('Roles'))
            CREATE INDEX [IX_Roles_RoleName]
            ON [dbo].[Roles]([RoleName]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Roles_IsActive' AND object_id = OBJECT_ID('Roles'))
            CREATE INDEX [IX_Roles_IsActive]
            ON [dbo].[Roles]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Roles_IsActive' AND object_id = OBJECT_ID('Roles'))
            DROP INDEX [IX_Roles_IsActive] ON [dbo].[Roles];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Roles_RoleName' AND object_id = OBJECT_ID('Roles'))
            DROP INDEX [IX_Roles_RoleName] ON [dbo].[Roles];

        -- ============================================================
        -- DROP TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Roles')
            DROP TABLE [dbo].[Roles];
    ");
        }
    }
}