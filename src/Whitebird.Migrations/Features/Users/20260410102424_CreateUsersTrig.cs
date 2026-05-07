using FluentMigrator;

namespace Whitebird.Migrations.Features.Users
{
    [Migration(20260410102424)]
    public class CreateUsersTrig : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Trigger untuk password history
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Users_PasswordHistory')
                    DROP TRIGGER TR_Users_PasswordHistory;
                GO
                
                CREATE TRIGGER TR_Users_PasswordHistory
                ON Users
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    IF UPDATE(PasswordHash)
                    BEGIN
                        INSERT INTO PasswordHistories (UserId, PasswordHash, ChangedDate)
                        SELECT i.UserId, i.PasswordHash, GETDATE()
                        FROM inserted i
                        INNER JOIN deleted d ON i.UserId = d.UserId
                        WHERE i.PasswordHash != d.PasswordHash;
                    END;
                END;
                GO

                -- Trigger untuk update ModifiedDate
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Users_UpdateModifiedDate')
                    DROP TRIGGER TR_Users_UpdateModifiedDate;
                GO
                
                CREATE TRIGGER TR_Users_UpdateModifiedDate
                ON Users
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    UPDATE u
                    SET ModifiedDate = GETDATE()
                    FROM Users u
                    INNER JOIN inserted i ON u.UserId = i.UserId;
                END;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Users_PasswordHistory')
                    DROP TRIGGER TR_Users_PasswordHistory;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Users_UpdateModifiedDate')
                    DROP TRIGGER TR_Users_UpdateModifiedDate;
                GO
            ");
        }
    }
}