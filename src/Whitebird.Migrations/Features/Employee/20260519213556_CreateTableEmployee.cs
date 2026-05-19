using FluentMigrator;

namespace Whitebird.Migrations.Features.Employee
{
    [Migration(20260519213556)]
    public class CreateTableEmployee : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        IF NOT EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'Employee' AND [xtype] = 'U')
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
        END;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_FullName')
            CREATE INDEX [IX_Employee_FullName]
            ON [dbo].[Employee]([FullName]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_DepartmentId')
            CREATE INDEX [IX_Employee_DepartmentId]
            ON [dbo].[Employee]([DepartmentId]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_OfficeId')
            CREATE INDEX [IX_Employee_OfficeId]
            ON [dbo].[Employee]([OfficeId]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_Position')
            CREATE INDEX [IX_Employee_Position]
            ON [dbo].[Employee]([Position]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_EmploymentStatus')
            CREATE INDEX [IX_Employee_EmploymentStatus]
            ON [dbo].[Employee]([EmploymentStatus]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_Email')
            CREATE INDEX [IX_Employee_Email]
            ON [dbo].[Employee]([Email]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_JoinDate')
            CREATE INDEX [IX_Employee_JoinDate]
            ON [dbo].[Employee]([JoinDate] DESC);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_IsActive')
            CREATE INDEX [IX_Employee_IsActive]
            ON [dbo].[Employee]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_IsActive')
            DROP INDEX [IX_Employee_IsActive] ON [dbo].[Employee];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_JoinDate')
            DROP INDEX [IX_Employee_JoinDate] ON [dbo].[Employee];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_Email')
            DROP INDEX [IX_Employee_Email] ON [dbo].[Employee];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_EmploymentStatus')
            DROP INDEX [IX_Employee_EmploymentStatus] ON [dbo].[Employee];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_Position')
            DROP INDEX [IX_Employee_Position] ON [dbo].[Employee];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_OfficeId')
            DROP INDEX [IX_Employee_OfficeId] ON [dbo].[Employee];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_DepartmentId')
            DROP INDEX [IX_Employee_DepartmentId] ON [dbo].[Employee];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Employee_FullName')
            DROP INDEX [IX_Employee_FullName] ON [dbo].[Employee];

        IF EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'Employee' AND [xtype] = 'U')
        BEGIN
            ALTER TABLE [dbo].[Employee] DROP CONSTRAINT [FK_Employee_Department];
            DROP TABLE [dbo].[Employee];
        END;
    ");
        }
    }
}
