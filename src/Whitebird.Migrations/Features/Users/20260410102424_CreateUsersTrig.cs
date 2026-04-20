using FluentMigrator;

namespace Whitebird.Migrations.Features.Users
{
    [Migration(20260410102424)]
    public class CreateUsersTrig : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Trigger untuk audit login
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Users_AuditLogin')
                    DROP TRIGGER TR_Users_AuditLogin;
                GO
                
                CREATE TRIGGER TR_Users_AuditLogin
                ON Users
                AFTER UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    IF UPDATE(LastLoginDate)
                    BEGIN
                        INSERT INTO ActivityLogs (
                            ReferenceTable,
                            ReferenceId,
                            ActivityType,
                            Description,
                            CreatedDate,
                            CreatedBy
                        )
                        SELECT 
                            'Users',
                            i.UserId,
                            'LOGIN',
                            'User ' + i.Username + ' logged in at ' + CAST(i.LastLoginDate AS NVARCHAR(50)),
                            GETDATE(),
                            'System'
                        FROM inserted i
                        INNER JOIN deleted d ON i.UserId = d.UserId
                        WHERE i.LastLoginDate IS NOT NULL 
                          AND (d.LastLoginDate IS NULL OR i.LastLoginDate != d.LastLoginDate);
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
                    
                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));

                    UPDATE Users
                    SET ModifiedDate = GETDATE(),
                        ModifiedBy = ISNULL(@ContextInfo, 'System')
                    FROM Users u
                    INNER JOIN inserted i ON u.UserId = i.UserId;
                END;
                GO

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

                    DECLARE @ContextInfo VARCHAR(128) = CAST(CONTEXT_INFO() AS VARCHAR(128));
                    
                    IF UPDATE(PasswordHash)
                    BEGIN
                        INSERT INTO PasswordHistories (UserId, PasswordHash, ChangedDate, ChangedBy)
                        SELECT i.UserId, i.PasswordHash, GETDATE(), ISNULL(@ContextInfo, 'System')
                        FROM inserted i
                        INNER JOIN deleted d ON i.UserId = d.UserId
                        WHERE i.PasswordHash != d.PasswordHash;
                    END;
                END;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Users_AuditLogin')
                    DROP TRIGGER TR_Users_AuditLogin;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Users_UpdateModifiedDate')
                    DROP TRIGGER TR_Users_UpdateModifiedDate;
                GO
                
                IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TR_Users_PasswordHistory')
                    DROP TRIGGER TR_Users_PasswordHistory;
                GO
            ");
        }
    }
}