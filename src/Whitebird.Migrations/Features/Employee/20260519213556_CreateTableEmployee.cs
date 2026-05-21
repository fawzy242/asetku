using FluentMigrator;

namespace Whitebird.Migrations.Features.Employee
{
    [Migration(20260519213556)]
    public class CreateTableEmployee : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Employee')
        BEGIN
            CREATE TABLE [dbo].[Employee] (
                [EmployeeId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_Employee_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_Employee_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Employee_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [EmployeeCode] NVARCHAR(50) NULL,
                [FullName] NVARCHAR(100) NULL,
                [Address] NVARCHAR(300) NULL,
                [DepartmentId] INT NULL,
                [Position] INT NULL,
                [EmploymentStatus] INT NULL,
                [PhoneNumber] NVARCHAR(20) NULL,
                [Email] NVARCHAR(100) NULL,
                [OfficeId] INT NULL,
                [JoinDate] DATE NULL,
                [ResignDate] DATE NULL,
                CONSTRAINT [PK_Employee] PRIMARY KEY ([EmployeeId]),
                CONSTRAINT [UQ_Employee_EmployeeCode] UNIQUE ([EmployeeCode]),
                CONSTRAINT [FK_Employee_Department]
                    FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Department]([DepartmentId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES (menggunakan OBJECT_ID yang lebih aman)
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_FullName' AND object_id = OBJECT_ID('Employee'))
            CREATE INDEX [IX_Employee_FullName]
            ON [dbo].[Employee]([FullName]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_DepartmentId' AND object_id = OBJECT_ID('Employee'))
            CREATE INDEX [IX_Employee_DepartmentId]
            ON [dbo].[Employee]([DepartmentId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_OfficeId' AND object_id = OBJECT_ID('Employee'))
            CREATE INDEX [IX_Employee_OfficeId]
            ON [dbo].[Employee]([OfficeId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_Position' AND object_id = OBJECT_ID('Employee'))
            CREATE INDEX [IX_Employee_Position]
            ON [dbo].[Employee]([Position]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_EmploymentStatus' AND object_id = OBJECT_ID('Employee'))
            CREATE INDEX [IX_Employee_EmploymentStatus]
            ON [dbo].[Employee]([EmploymentStatus]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_Email' AND object_id = OBJECT_ID('Employee'))
            CREATE INDEX [IX_Employee_Email]
            ON [dbo].[Employee]([Email]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_JoinDate' AND object_id = OBJECT_ID('Employee'))
            CREATE INDEX [IX_Employee_JoinDate]
            ON [dbo].[Employee]([JoinDate] DESC);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_IsActive' AND object_id = OBJECT_ID('Employee'))
            CREATE INDEX [IX_Employee_IsActive]
            ON [dbo].[Employee]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_IsActive' AND object_id = OBJECT_ID('Employee'))
            DROP INDEX [IX_Employee_IsActive] ON [dbo].[Employee];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_JoinDate' AND object_id = OBJECT_ID('Employee'))
            DROP INDEX [IX_Employee_JoinDate] ON [dbo].[Employee];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_Email' AND object_id = OBJECT_ID('Employee'))
            DROP INDEX [IX_Employee_Email] ON [dbo].[Employee];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_EmploymentStatus' AND object_id = OBJECT_ID('Employee'))
            DROP INDEX [IX_Employee_EmploymentStatus] ON [dbo].[Employee];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_Position' AND object_id = OBJECT_ID('Employee'))
            DROP INDEX [IX_Employee_Position] ON [dbo].[Employee];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_OfficeId' AND object_id = OBJECT_ID('Employee'))
            DROP INDEX [IX_Employee_OfficeId] ON [dbo].[Employee];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_DepartmentId' AND object_id = OBJECT_ID('Employee'))
            DROP INDEX [IX_Employee_DepartmentId] ON [dbo].[Employee];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Employee_FullName' AND object_id = OBJECT_ID('Employee'))
            DROP INDEX [IX_Employee_FullName] ON [dbo].[Employee];

        -- ============================================================
        -- DROP FOREIGN KEY & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Employee')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('Employee') AND name = 'FK_Employee_Department')
                ALTER TABLE [dbo].[Employee] DROP CONSTRAINT [FK_Employee_Department];
            
            DROP TABLE [dbo].[Employee];
        END
    ");
        }
    }
}