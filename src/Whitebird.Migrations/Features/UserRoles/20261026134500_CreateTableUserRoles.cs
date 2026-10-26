using FluentMigrator;

namespace Whitebird.Migrations.Features.UserRoles
{
    [Migration(20261026134500)]
    public class CreateTableUserRoles : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserRoles')
        BEGIN
            CREATE TABLE [dbo].[UserRoles] (
                [UserRoleId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_UserRoles_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_UserRoles_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_UserRoles_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [UserId] INT NOT NULL,
                [RoleId] INT NOT NULL,
                CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserRoleId]),
                CONSTRAINT [UQ_UserRoles_UserId_RoleId] UNIQUE ([UserId], [RoleId]),
                CONSTRAINT [FK_UserRoles_Users]
                    FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]),
                CONSTRAINT [FK_UserRoles_Roles]
                    FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles]([RoleId])
            );
        END

        -- ============================================================
        -- CREATE INDEXES
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserRoles_RoleId' AND object_id = OBJECT_ID('UserRoles'))
            CREATE INDEX [IX_UserRoles_RoleId]
            ON [dbo].[UserRoles]([RoleId]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserRoles_UserId_IsActive' AND object_id = OBJECT_ID('UserRoles'))
            CREATE INDEX [IX_UserRoles_UserId_IsActive]
            ON [dbo].[UserRoles]([UserId], [IsActive])
            WHERE [IsActive] = 1;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserRoles_IsActive' AND object_id = OBJECT_ID('UserRoles'))
            CREATE INDEX [IX_UserRoles_IsActive]
            ON [dbo].[UserRoles]([IsActive])
            WHERE [IsActive] = 1;
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserRoles_IsActive' AND object_id = OBJECT_ID('UserRoles'))
            DROP INDEX [IX_UserRoles_IsActive] ON [dbo].[UserRoles];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserRoles_UserId_IsActive' AND object_id = OBJECT_ID('UserRoles'))
            DROP INDEX [IX_UserRoles_UserId_IsActive] ON [dbo].[UserRoles];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_UserRoles_RoleId' AND object_id = OBJECT_ID('UserRoles'))
            DROP INDEX [IX_UserRoles_RoleId] ON [dbo].[UserRoles];

        -- ============================================================
        -- DROP FOREIGN KEYS & TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserRoles')
        BEGIN
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('UserRoles') AND name = 'FK_UserRoles_Roles')
                ALTER TABLE [dbo].[UserRoles] DROP CONSTRAINT [FK_UserRoles_Roles];
            
            IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE parent_object_id = OBJECT_ID('UserRoles') AND name = 'FK_UserRoles_Users')
                ALTER TABLE [dbo].[UserRoles] DROP CONSTRAINT [FK_UserRoles_Users];
            
            DROP TABLE [dbo].[UserRoles];
        END
    ");
        }
    }
}