using FluentMigrator;

namespace Whitebird.Migrations.Features.Employee
{
    [Migration(20260410102401)]
    public class CreateEmployeeTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Employee' AND xtype = 'U')
                BEGIN
                    CREATE TABLE Employee (
                        EmployeeId INT NOT NULL CONSTRAINT DF_Employee_EmployeeId DEFAULT NEXT VALUE FOR Seq_EmployeeId,
                        EmployeeCode NVARCHAR(50) NOT NULL UNIQUE,
                        FullName NVARCHAR(100) NOT NULL,
                        Department NVARCHAR(50) NULL,
                        Position NVARCHAR(50) NULL,
                        Division NVARCHAR(50) NULL,
                        Branch NVARCHAR(50) NULL,
                        CostCenter NVARCHAR(50) NULL,
                        PhoneNumber NVARCHAR(20) NULL,
                        Email NVARCHAR(100) NULL,
                        OfficeLocation NVARCHAR(100) NULL,
                        EmploymentStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_Employee_EmploymentStatus DEFAULT 'Active',
                        JoinDate DATETIME NULL,
                        ResignDate DATETIME NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_Employee_IsActive DEFAULT 1,
                        CreatedDate DATETIME NOT NULL CONSTRAINT DF_Employee_CreatedDate DEFAULT GETDATE(),
                        CreatedBy NVARCHAR(50) NOT NULL CONSTRAINT DF_Employee_CreatedBy DEFAULT 'System',
                        ModifiedDate DATETIME NULL,
                        ModifiedBy NVARCHAR(50) NULL,
                        
                        CONSTRAINT PK_Employee PRIMARY KEY (EmployeeId)
                    );
                END;
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employee_EmployeeCode' AND object_id = OBJECT_ID('Employee'))
                    CREATE INDEX IX_Employee_EmployeeCode ON Employee(EmployeeCode);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employee_Department' AND object_id = OBJECT_ID('Employee'))
                    CREATE INDEX IX_Employee_Department ON Employee(Department);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employee_EmploymentStatus' AND object_id = OBJECT_ID('Employee'))
                    CREATE INDEX IX_Employee_EmploymentStatus ON Employee(EmploymentStatus);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employee_IsActive' AND object_id = OBJECT_ID('Employee'))
                    CREATE INDEX IX_Employee_IsActive ON Employee(IsActive);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Employee_Email' AND object_id = OBJECT_ID('Employee'))
                    CREATE INDEX IX_Employee_Email ON Employee(Email);
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name = 'Employee' AND xtype = 'U')
                    DROP TABLE Employee;
                GO
            ");
        }
    }
}
