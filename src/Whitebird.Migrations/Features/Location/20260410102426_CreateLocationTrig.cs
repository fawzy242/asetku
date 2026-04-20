using FluentMigrator;

namespace Whitebird.Migrations.Features.Location
{
    [Migration(20260410102426)]
    public class CreateLocationTrig : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Trigger untuk auto-generate LocationCode
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Location_GenerateCode')
                    DROP TRIGGER TR_Location_GenerateCode;
                GO
                
                CREATE TRIGGER TR_Location_GenerateCode
                ON Location
                INSTEAD OF INSERT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    INSERT INTO Location (
                        LocationId, LocationCode, LocationName, LocationType, Address,
                        City, ParentLocationId, IsActive, CreatedDate,
                        CreatedBy, ModifiedDate, ModifiedBy
                    )
                    SELECT 
                        NEXT VALUE FOR Seq_LocationId,
                        CASE 
                            WHEN i.LocationCode IS NULL OR i.LocationCode = '' 
                            THEN 'LOC-' + RIGHT('000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000 AS NVARCHAR(6)), 6)
                            ELSE i.LocationCode
                        END,
                        i.LocationName, i.LocationType, i.Address,
                        i.City, i.ParentLocationId, i.IsActive,
                        i.CreatedDate, i.CreatedBy, i.ModifiedDate, i.ModifiedBy
                    FROM inserted i;
                END;
                GO
                
                -- Trigger untuk prevent circular reference
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Location_PreventCircularReference')
                    DROP TRIGGER TR_Location_PreventCircularReference;
                GO
                
                CREATE TRIGGER TR_Location_PreventCircularReference
ON Location
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @HasCircularReference BIT = 0;
    
    ;WITH CTE AS (
        SELECT LocationId, ParentLocationId
        FROM Location
        WHERE LocationId IN (SELECT LocationId FROM inserted)
        UNION ALL
        SELECT l.LocationId, l.ParentLocationId
        FROM Location l
        INNER JOIN CTE ON l.LocationId = CTE.ParentLocationId
    )
    SELECT @HasCircularReference = 1
    FROM CTE
    WHERE LocationId = ParentLocationId;
    
    IF @HasCircularReference = 1
    BEGIN
        RAISERROR('Circular reference detected in location hierarchy', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
                GO

                -- Trigger untuk update ModifiedDate
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Location_UpdateModifiedDate')
                    DROP TRIGGER TR_Location_UpdateModifiedDate;
                GO
                
                CREATE TRIGGER TR_Location_UpdateModifiedDate
                ON Location
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));
                    
                    UPDATE Location
                    SET ModifiedDate = GETDATE(),
                        ModifiedBy = ISNULL(@ContextInfo, 'System')
                    FROM Location l
                    INNER JOIN inserted i ON l.LocationId = i.LocationId;
                END;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Location_GenerateCode')
                    DROP TRIGGER TR_Location_GenerateCode;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Location_PreventCircularReference')
                    DROP TRIGGER TR_Location_PreventCircularReference;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Location_UpdateModifiedDate')
                    DROP TRIGGER TR_Location_UpdateModifiedDate;
                GO
            ");
        }
    }
}