using FluentMigrator;

namespace Whitebird.Migrations.Features.User
{
    [Migration(20260519213742)]
    public class SeedUser : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Users] WHERE [Username] = 'admin')
        BEGIN
            INSERT INTO [dbo].[Users] (
                [Username], [Email], [PasswordHash], [FullName], [PhoneNumber],
                [IsActive], [IsLocked], [CreatedBy]
            )
            VALUES (
                'admin',
                'admin@company.com',
                '$2a$12$LJ3m4ys3Lk0TSwHCpNqrAOq7MqE3qOZxqJvGr6vQxqj6lFqCqJ7e',
                'System Administrator',
                '+62-000-0000-0000',
                1, 0, 'System'
            );

            INSERT INTO [dbo].[Users] (
                [Username], [Email], [PasswordHash], [FullName], [PhoneNumber],
                [IsActive], [IsLocked], [CreatedBy]
            )
            VALUES (
                'it.support',
                'it.support@company.com',
                '$2a$12$LJ3m4ys3Lk0TSwHCpNqrAOq7MqE3qOZxqJvGr6vQxqj6lFqCqJ7e',
                'IT Support',
                '+62-000-0000-0001',
                1, 0, 'System'
            );

            INSERT INTO [dbo].[Users] (
                [Username], [Email], [PasswordHash], [FullName], [PhoneNumber],
                [IsActive], [IsLocked], [CreatedBy]
            )
            VALUES (
                'asset.manager',
                'asset.manager@company.com',
                '$2a$12$LJ3m4ys3Lk0TSwHCpNqrAOq7MqE3qOZxqJvGr6vQxqj6lFqCqJ7e',
                'Asset Manager',
                '+62-000-0000-0002',
                1, 0, 'System'
            );
        END;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"");
        }
    }
}
