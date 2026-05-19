using FluentMigrator;

namespace Whitebird.Migrations.Features.Category
{
    [Migration(20260519213438)]
    public class CreateTableCategory : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        IF NOT EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'Category' AND [xtype] = 'U')
        BEGIN
            CREATE TABLE [dbo].[Category] (
                [CategoryId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_Category_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_Category_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Category_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [CategoryCode] NVARCHAR(100) NULL,
                [CategoryName] NVARCHAR(100) NULL,
                [Description] NVARCHAR(500) NULL,
                [ParentCategoryId] INT NULL,
                CONSTRAINT [PK_Category] PRIMARY KEY ([CategoryId]),
                CONSTRAINT [FK_Category_ParentCategory]
                    FOREIGN KEY ([ParentCategoryId]) REFERENCES [dbo].[Category]([CategoryId])
            );
        END;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Category_CategoryName')
            CREATE INDEX [IX_Category_CategoryName]
            ON [dbo].[Category]([CategoryName]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Category_CategoryCode')
            CREATE INDEX [IX_Category_CategoryCode]
            ON [dbo].[Category]([CategoryCode]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Category_ParentCategoryId')
            CREATE INDEX [IX_Category_ParentCategoryId]
            ON [dbo].[Category]([ParentCategoryId]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Category_IsActive')
            CREATE INDEX [IX_Category_IsActive]
            ON [dbo].[Category]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Category_IsActive')
            DROP INDEX [IX_Category_IsActive] ON [dbo].[Category];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Category_ParentCategoryId')
            DROP INDEX [IX_Category_ParentCategoryId] ON [dbo].[Category];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Category_CategoryCode')
            DROP INDEX [IX_Category_CategoryCode] ON [dbo].[Category];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Category_CategoryName')
            DROP INDEX [IX_Category_CategoryName] ON [dbo].[Category];

        IF EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'Category' AND [xtype] = 'U')
        BEGIN
            ALTER TABLE [dbo].[Category] DROP CONSTRAINT [FK_Category_ParentCategory];
            DROP TABLE [dbo].[Category];
        END;
    ");
        }
    }
}
