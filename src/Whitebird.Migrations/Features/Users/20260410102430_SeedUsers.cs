using FluentMigrator;

namespace Whitebird.Migrations.Features.Users
{
    [Migration(20260410102430)]
    public class SeedUsers : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                -- Insert default admin user jika belum ada
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
                BEGIN
                    INSERT INTO Users (
                        Username, 
                        Email, 
                        PasswordHash, 
                        FullName, 
                        RoleId,
                        IsActive, 
                        CreatedBy
                    )
                    VALUES (
                        'admin',
                        'admin@whitebird.local',
                        'AQAAAAIAAYagAAAAENaCGkFkFpDV+5qHKBqL2RVKdQLXqHBx8vPKXqBx8vM=', -- Default: Admin@123
                        'System Administrator',
                        'Admin',
                        1,
                        'Migration'
                    );
                END;
                
                -- Insert default roles jika diperlukan (opsional)
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'user')
                BEGIN
                    INSERT INTO Users (
                        Username, 
                        Email, 
                        PasswordHash, 
                        FullName, 
                        RoleId,
                        IsActive, 
                        CreatedBy
                    )
                    VALUES (
                        'user',
                        'user@whitebird.local',
                        'AQAAAAIAAYagAAAAENaCGkFkFpDV+5qHKBqL2RVKdQLXqHBx8vPKXqBx8vM=', -- Default: User@123
                        'Regular User',
                        'User',
                        1,
                        'Migration'
                    );
                END;
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                DELETE FROM Users WHERE Username IN ('admin', 'user') AND CreatedBy = 'Migration';
            ");
        }
    }
}