using FluentMigrator;

namespace Whitebird.Migrations.Features.UserRoles
{
    [Migration(20261026140553)]
    public class SeedUserRoles : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- Assign the ADMIN role to the existing 'admin' seed user.
        -- To also grant ADMIN to 'it.support' and/or 'asset.manager',
        -- duplicate the block below and change the WHERE [Username] = '...' filter.
        IF NOT EXISTS (
            SELECT 1
            FROM [dbo].[UserRoles] ur
            INNER JOIN [dbo].[Users] u ON u.[UserId] = ur.[UserId]
            INNER JOIN [dbo].[Roles] r ON r.[RoleId] = ur.[RoleId]
            WHERE u.[Username] = 'admin' AND r.[RoleCode] = 'ADMIN'
        )
        BEGIN
            INSERT INTO [dbo].[UserRoles] ([UserId], [RoleId])
            SELECT u.[UserId], r.[RoleId]
            FROM [dbo].[Users] u
            CROSS JOIN [dbo].[Roles] r
            WHERE u.[Username] = 'admin' AND r.[RoleCode] = 'ADMIN';
        END;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        DELETE FROM [dbo].[UserRoles]
        WHERE [UserId] IN (SELECT [UserId] FROM [dbo].[Users] WHERE [Username] = 'admin')
          AND [RoleId] IN (SELECT [RoleId] FROM [dbo].[Roles] WHERE [RoleCode] = 'ADMIN');
    ");
        }
    }
}