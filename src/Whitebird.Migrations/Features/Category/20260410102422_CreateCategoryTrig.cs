using FluentMigrator;

namespace Whitebird.Migrations.Features.Category
{
    [Migration(20260410102422)]
    public class CreateCategoryTrig : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Trigger untuk prevent circular reference
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Category_PreventCircularReference')
                    DROP TRIGGER TR_Category_PreventCircularReference;
                GO
                
                CREATE TRIGGER TR_Category_PreventCircularReference
                ON Category
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    IF EXISTS (
                        SELECT 1 FROM inserted i
                        WHERE i.ParentCategoryId IS NOT NULL
                          AND i.ParentCategoryId = i.CategoryId
                    )
                    BEGIN
                        RAISERROR('Category cannot be parent of itself', 16, 1);
                        ROLLBACK TRANSACTION;
                        RETURN;
                    END;

                    IF EXISTS (
                        SELECT 1 FROM Category c
                        INNER JOIN inserted i ON c.CategoryId = i.ParentCategoryId
                        WHERE c.ParentCategoryId = i.CategoryId
                    )
                    BEGIN
                        RAISERROR('Circular reference detected in category hierarchy', 16, 1);
                        ROLLBACK TRANSACTION;
                        RETURN;
                    END;
                END;
                GO

                -- Trigger untuk update ModifiedDate
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Category_UpdateModifiedDate')
                    DROP TRIGGER TR_Category_UpdateModifiedDate;
                GO
                
                CREATE TRIGGER TR_Category_UpdateModifiedDate
                ON Category
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    UPDATE c
                    SET ModifiedDate = GETDATE()
                    FROM Category c
                    INNER JOIN inserted i ON c.CategoryId = i.CategoryId;
                END;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Category_PreventCircularReference')
                    DROP TRIGGER TR_Category_PreventCircularReference;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Category_UpdateModifiedDate')
                    DROP TRIGGER TR_Category_UpdateModifiedDate;
                GO
            ");
        }
    }
}