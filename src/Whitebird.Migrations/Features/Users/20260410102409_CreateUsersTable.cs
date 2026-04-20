using FluentMigrator;

namespace Whitebird.Migrations.Features.Users
{
    [Migration(20260410102409)]
    public class CreateUsersTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Users' AND xtype = 'U')
                BEGIN
                    CREATE TABLE Users (
                        UserId INT NOT NULL CONSTRAINT DF_Users_UserId DEFAULT NEXT VALUE FOR Seq_UserId,
                        Username NVARCHAR(100) NOT NULL UNIQUE,
                        Email NVARCHAR(100) NOT NULL UNIQUE,
                        PasswordHash NVARCHAR(255) NOT NULL,
                        FullName NVARCHAR(100) NOT NULL,
                        PhoneNumber NVARCHAR(20) NULL,
                        RoleId NVARCHAR(50) NULL,
                        SessionToken NVARCHAR(255) NULL,
                        SessionExpiry DATETIME NULL,
                        IsLocked BIT NOT NULL CONSTRAINT DF_Users_IsLocked DEFAULT 0,
                        ResetToken NVARCHAR(255) NULL,
                        ResetTokenExpiry DATETIME NULL,
                        LastLoginDate DATETIME NULL,
                        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT 1,
                        CreatedDate DATETIME NOT NULL CONSTRAINT DF_Users_CreatedDate DEFAULT GETDATE(),
                        CreatedBy NVARCHAR(50) NOT NULL CONSTRAINT DF_Users_CreatedBy DEFAULT 'System',
                        ModifiedDate DATETIME NULL,
                        ModifiedBy NVARCHAR(50) NULL,
                        
                        CONSTRAINT PK_Users PRIMARY KEY (UserId)
                    );
                END;
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username' AND object_id = OBJECT_ID('Users'))
                    CREATE INDEX IX_Users_Username ON Users(Username);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
                    CREATE INDEX IX_Users_Email ON Users(Email);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_IsActive' AND object_id = OBJECT_ID('Users'))
                    CREATE INDEX IX_Users_IsActive ON Users(IsActive);
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_RoleId' AND object_id = OBJECT_ID('Users'))
                    CREATE INDEX IX_Users_RoleId ON Users(RoleId);
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name = 'Users' AND xtype = 'U')
                    DROP TABLE Users;
                GO
            ");
        }
    }
}
