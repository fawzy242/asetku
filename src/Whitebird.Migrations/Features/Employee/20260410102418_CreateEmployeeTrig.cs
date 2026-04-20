using FluentMigrator;

namespace Whitebird.Migrations.Features.Employee
{
    [Migration(20260410102418)]
    public class CreateEmployeeTrig : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Trigger untuk auto-generate EmployeeCode
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Employee_GenerateCode')
                    DROP TRIGGER TR_Employee_GenerateCode;
                GO
                
                CREATE TRIGGER TR_Employee_GenerateCode
                ON Employee
                INSTEAD OF INSERT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    INSERT INTO Employee (
                        EmployeeId, EmployeeCode, FullName, Department, Position, Division,
                        Branch, CostCenter, PhoneNumber, Email, OfficeLocation,
                        EmploymentStatus, JoinDate, ResignDate, IsActive,
                        CreatedDate, CreatedBy, ModifiedDate, ModifiedBy
                    )
                    SELECT 
                        NEXT VALUE FOR Seq_EmployeeId,
                        CASE 
                            WHEN i.EmployeeCode IS NULL OR i.EmployeeCode = '' 
                            THEN 'EMP-' + RIGHT('000000' + CAST(ABS(CHECKSUM(NEWID())) % 1000000 AS NVARCHAR(6)), 6)
                            ELSE i.EmployeeCode
                        END,
                        i.FullName, i.Department, i.Position, i.Division,
                        i.Branch, i.CostCenter, i.PhoneNumber, i.Email, i.OfficeLocation,
                        i.EmploymentStatus, i.JoinDate, i.ResignDate, i.IsActive,
                        i.CreatedDate, i.CreatedBy, i.ModifiedDate, i.ModifiedBy
                    FROM inserted i;
                END;
                GO

                -- Trigger untuk update ModifiedDate
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Employee_UpdateModifiedDate')
                    DROP TRIGGER TR_Employee_UpdateModifiedDate;
                GO
                
                CREATE TRIGGER TR_Employee_UpdateModifiedDate
                ON Employee
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));
                    
                    UPDATE Employee
                    SET ModifiedDate = GETDATE(),
                        ModifiedBy = ISNULL(@ContextInfo, 'System')
                    FROM Employee e
                    INNER JOIN inserted i ON e.EmployeeId = i.EmployeeId;
                END;
                GO

                -- Trigger untuk prevent delete jika masih memegang asset
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Employee_PreventDeleteIfHasAsset')
                    DROP TRIGGER TR_Employee_PreventDeleteIfHasAsset;
                GO
                
                CREATE TRIGGER TR_Employee_PreventDeleteIfHasAsset
                ON Employee
                INSTEAD OF DELETE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));
                    
                    IF EXISTS (
                        SELECT 1 FROM Asset a
                        INNER JOIN deleted d ON a.CurrentHolderId = d.EmployeeId
                        WHERE a.Status = 'Assigned'
                    )
                    BEGIN
                        RAISERROR('Cannot delete employee who is currently holding Asset', 16, 1);
                        RETURN;
                    END;
                    
                    -- Soft delete instead of hard delete
                    UPDATE Employee
                    SET IsActive = 0,
                        EmploymentStatus = 'Resigned',
                        ResignDate = GETDATE(),
                        ModifiedDate = GETDATE(),
                        ModifiedBy = ISNULL(@ContextInfo, 'System')
                    FROM Employee e
                    INNER JOIN deleted d ON e.EmployeeId = d.EmployeeId;
                END;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Employee_GenerateCode')
                    DROP TRIGGER TR_Employee_GenerateCode;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Employee_UpdateModifiedDate')
                    DROP TRIGGER TR_Employee_UpdateModifiedDate;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Employee_PreventDeleteIfHasAsset')
                    DROP TRIGGER TR_Employee_PreventDeleteIfHasAsset;
                GO
            ");
        }
    }
}