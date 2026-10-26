using FluentMigrator;

namespace Whitebird.Migrations.Features.Roles
{
    [Migration(20261026140545)]
    public class SeedRoles : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        IF NOT EXISTS (SELECT 1 FROM [dbo].[Roles] WHERE [RoleCode] = 'ADMIN')
        BEGIN
            INSERT INTO [dbo].[Roles] ([RoleCode], [RoleName], [Description])
            VALUES
                ('ADMIN', 'Administrator', 'Full access to all system features and configurations');
        END;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- Defensive cleanup: remove any UserRoles mappings referencing the seeded role
        -- (FK constraint would otherwise block the DELETE below in edge cases).
        DELETE FROM [dbo].[UserRoles]
        WHERE [RoleId] IN (SELECT [RoleId] FROM [dbo].[Roles] WHERE [RoleCode] = 'ADMIN');

        DELETE FROM [dbo].[Roles]
        WHERE [RoleCode] = 'ADMIN';
    ");
        }
    }
}