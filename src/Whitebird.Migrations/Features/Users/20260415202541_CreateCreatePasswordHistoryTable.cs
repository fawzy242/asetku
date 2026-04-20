using FluentMigrator;

namespace Whitebird.Migrations.Features.Users
{
    [Migration(20260415202541)]
    public class CreateCreatePasswordHistoryTable : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'PasswordHistories' AND xtype = 'U')
                BEGIN
                    CREATE TABLE PasswordHistories (
                        HistoryId INT IDENTITY(1,1) PRIMARY KEY,
                        UserId INT NOT NULL,
                        PasswordHash NVARCHAR(255) NOT NULL,
                        ChangedDate DATETIME NOT NULL DEFAULT GETDATE(),
                        ChangedBy NVARCHAR(50) NULL,
                        
                        CONSTRAINT FK_PasswordHistories_User FOREIGN KEY (UserId) 
                            REFERENCES Users(UserId)
                    );
                END;
                GO
                
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PasswordHistories_UserId' AND object_id = OBJECT_ID('PasswordHistories'))
                    CREATE INDEX IX_PasswordHistories_UserId ON PasswordHistories(UserId);
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name = 'PasswordHistories' AND xtype = 'U')
                    DROP TABLE PasswordHistories;
                GO
            ");
        }
    }
}
