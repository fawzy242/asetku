using FluentMigrator;

namespace Whitebird.Migrations.Features.Department
{
    [Migration(20260519213419)]
    public class CreateTableDepartment : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        IF NOT EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'Department' AND [xtype] = 'U')
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
        END;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Department_DepartmentName')
            CREATE INDEX [IX_Department_DepartmentName]
            ON [dbo].[Department]([DepartmentName]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Department_DepartmentCode')
            CREATE INDEX [IX_Department_DepartmentCode]
            ON [dbo].[Department]([DepartmentCode]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Department_IsActive')
            CREATE INDEX [IX_Department_IsActive]
            ON [dbo].[Department]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Department_IsActive')
            DROP INDEX [IX_Department_IsActive] ON [dbo].[Department];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Department_DepartmentCode')
            DROP INDEX [IX_Department_DepartmentCode] ON [dbo].[Department];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Department_DepartmentName')
            DROP INDEX [IX_Department_DepartmentName] ON [dbo].[Department];

        IF EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'Department' AND [xtype] = 'U')
            DROP TABLE [dbo].[Department];
    ");
        }
    }
}
