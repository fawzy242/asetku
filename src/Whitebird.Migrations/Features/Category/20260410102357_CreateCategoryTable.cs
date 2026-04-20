using FluentMigrator;

namespace Whitebird.Migrations.Features.Category
{
    [Migration(20260410102357)]
    public class CreateCategoryTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Category' AND xtype = 'U')
                BEGIN
                    CREATE TABLE Category (
                        CategoryId INT NOT NULL CONSTRAINT DF_Category_CategoryId DEFAULT NEXT VALUE FOR Seq_CategoryId,
                        CategoryName NVARCHAR(100) NOT NULL,
                        Description NVARCHAR(500) NULL,
                        ParentCategoryId INT NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_Category_IsActive DEFAULT 1,
                        CreatedDate DATETIME NOT NULL CONSTRAINT DF_Category_CreatedDate DEFAULT GETDATE(),
                        CreatedBy NVARCHAR(50) NOT NULL CONSTRAINT DF_Category_CreatedBy DEFAULT 'System',
                        ModifiedDate DATETIME NULL,
                        ModifiedBy NVARCHAR(50) NULL,
                        
                        CONSTRAINT PK_Category PRIMARY KEY (CategoryId),
                        CONSTRAINT FK_Category_ParentCategory FOREIGN KEY (ParentCategoryId) 
                            REFERENCES Category(CategoryId)
                    );
                END;
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Category_ParentCategoryId' AND object_id = OBJECT_ID('Category'))
                    CREATE INDEX IX_Category_ParentCategoryId ON Category(ParentCategoryId);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Category_IsActive' AND object_id = OBJECT_ID('Category'))
                    CREATE INDEX IX_Category_IsActive ON Category(IsActive);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Category_CategoryName' AND object_id = OBJECT_ID('Category'))
                    CREATE INDEX IX_Category_CategoryName ON Category(CategoryName);
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name = 'Category' AND xtype = 'U')
                    DROP TABLE Category;
                GO
            ");
        }
    }
}
