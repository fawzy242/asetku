using FluentMigrator;

namespace Whitebird.Migrations.Features.Location
{
    [Migration(20260410102403)]
    public class CreateLocationTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Location' AND xtype = 'U')
                BEGIN
                    CREATE TABLE Location (
                        LocationId INT NOT NULL CONSTRAINT DF_Location_LocationId DEFAULT NEXT VALUE FOR Seq_LocationId,
                        LocationCode NVARCHAR(50) NOT NULL UNIQUE,
                        LocationName NVARCHAR(100) NOT NULL,
                        LocationType NVARCHAR(50) NULL,
                        Address NVARCHAR(500) NULL,
                        City NVARCHAR(100) NULL,
                        ParentLocationId INT NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_Location_IsActive DEFAULT 1,
                        CreatedDate DATETIME NOT NULL CONSTRAINT DF_Location_CreatedDate DEFAULT GETDATE(),
                        CreatedBy NVARCHAR(50) NOT NULL CONSTRAINT DF_Location_CreatedBy DEFAULT 'System',
                        ModifiedDate DATETIME NULL,
                        ModifiedBy NVARCHAR(50) NULL,
                        
                        CONSTRAINT PK_Location PRIMARY KEY (LocationId),
                        CONSTRAINT FK_Location_ParentLocation FOREIGN KEY (ParentLocationId) 
                            REFERENCES Location(LocationId)
                    );
                END;
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Location_LocationCode' AND object_id = OBJECT_ID('Location'))
                    CREATE INDEX IX_Location_LocationCode ON Location(LocationCode);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Location_LocationName' AND object_id = OBJECT_ID('Location'))
                    CREATE INDEX IX_Location_LocationName ON Location(LocationName);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Location_ParentLocationId' AND object_id = OBJECT_ID('Location'))
                    CREATE INDEX IX_Location_ParentLocationId ON Location(ParentLocationId);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Location_IsActive' AND object_id = OBJECT_ID('Location'))
                    CREATE INDEX IX_Location_IsActive ON Location(IsActive);
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name = 'Location' AND xtype = 'U')
                    DROP TABLE Location;
                GO
            ");
        }
    }
}
