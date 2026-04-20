using FluentMigrator;

namespace Whitebird.Migrations.Features.Category
{
    [Migration(20260410102422)]
    public class CreateCategoryTrig : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Trigger untuk prevent circular reference di hierarchy category
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Category_PreventCircularReference')
                    DROP TRIGGER TR_Category_PreventCircularReference;
                GO
                
CREATE TRIGGER TR_Category_PreventCircularReference
ON Category
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @HasCircularReference BIT = 0;
    
    ;WITH CTE AS (
        SELECT CategoryId, ParentCategoryId
        FROM Category
        WHERE CategoryId IN (SELECT CategoryId FROM inserted)
        UNION ALL
        SELECT c.CategoryId, c.ParentCategoryId
        FROM Category c
        INNER JOIN CTE ON c.CategoryId = CTE.ParentCategoryId
    )
    SELECT @HasCircularReference = 1
    FROM CTE
    WHERE CategoryId = ParentCategoryId;
    
    IF @HasCircularReference = 1
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

                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));
                    
                    UPDATE Category
                    SET ModifiedDate = GETDATE(),
                        ModifiedBy = ISNULL(@ContextInfo, 'System')
                    FROM Category c
                    INNER JOIN inserted i ON c.CategoryId = i.CategoryId;
                END;
                GO

                -- Trigger untuk prevent delete if has child Category or Asset
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Category_PreventDeleteIfUsed')
                    DROP TRIGGER TR_Category_PreventDeleteIfUsed;
                GO
                
                CREATE TRIGGER TR_Category_PreventDeleteIfUsed
                ON Category
                INSTEAD OF DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));
                    
                    IF EXISTS (
                        SELECT 1 FROM Category c
                        INNER JOIN deleted d ON c.ParentCategoryId = d.CategoryId
                    )
                    BEGIN
                        RAISERROR('Cannot delete category that has child Category', 16, 1);
                        RETURN;
                    END;
                    
                    IF EXISTS (
                        SELECT 1 FROM Asset a
                        INNER JOIN deleted d ON a.CategoryId = d.CategoryId
                        WHERE a.IsActive = 1
                    )
                    BEGIN
                        RAISERROR('Cannot delete category that has active Asset', 16, 1);
                        RETURN;
                    END;
                    
                    -- Soft delete
                    UPDATE Category
                    SET IsActive = 0,
                        ModifiedDate = GETDATE(),
                        ModifiedBy = ISNULL(@ContextInfo, 'System')
                    FROM Category c
                    INNER JOIN deleted d ON c.CategoryId = d.CategoryId;
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
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Category_PreventDeleteIfUsed')
                    DROP TRIGGER TR_Category_PreventDeleteIfUsed;
                GO
            ");
        }
    }
}