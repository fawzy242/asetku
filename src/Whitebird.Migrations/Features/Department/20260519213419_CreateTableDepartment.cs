using FluentMigrator;

namespace Whitebird.Migrations.Features.Department
{
    [Migration(20260519213419)]
    public class CreateTableDepartment : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Department')
        BEGIN
            CREATE TABLE [dbo].[Department] (
                [DepartmentId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_Department_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_Department_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Department_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [DepartmentCode] NVARCHAR(100) NULL,
                [DepartmentName] NVARCHAR(100) NULL,
                [Description] NVARCHAR(500) NULL,
                CONSTRAINT [PK_Department] PRIMARY KEY ([DepartmentId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES (menggunakan OBJECT_ID yang lebih aman)
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Department_DepartmentName' AND object_id = OBJECT_ID('Department'))
            CREATE INDEX [IX_Department_DepartmentName]
            ON [dbo].[Department]([DepartmentName]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Department_DepartmentCode' AND object_id = OBJECT_ID('Department'))
            CREATE INDEX [IX_Department_DepartmentCode]
            ON [dbo].[Department]([DepartmentCode]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Department_IsActive' AND object_id = OBJECT_ID('Department'))
            CREATE INDEX [IX_Department_IsActive]
            ON [dbo].[Department]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Department_IsActive' AND object_id = OBJECT_ID('Department'))
            DROP INDEX [IX_Department_IsActive] ON [dbo].[Department];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Department_DepartmentCode' AND object_id = OBJECT_ID('Department'))
            DROP INDEX [IX_Department_DepartmentCode] ON [dbo].[Department];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Department_DepartmentName' AND object_id = OBJECT_ID('Department'))
            DROP INDEX [IX_Department_DepartmentName] ON [dbo].[Department];

        -- ============================================================
        -- DROP TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Department')
            DROP TABLE [dbo].[Department];
    ");
        }
    }
}