using FluentMigrator;

namespace Whitebird.Migrations.Features.User
{
    [Migration(20260519213352)]
    public class CreateTableUser : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        -- ============================================================
        -- CREATE TABLE
        -- ============================================================
        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
        BEGIN
            CREATE TABLE [dbo].[Users] (
                [UserId] INT IDENTITY(1,1) NOT NULL,
                [IsActive] BIT NOT NULL CONSTRAINT [DF_Users_IsActive] DEFAULT 1,
                [CreatedDate] DATETIME NOT NULL CONSTRAINT [DF_Users_CreatedDate] DEFAULT GETDATE(),
                [CreatedBy] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Users_CreatedBy] DEFAULT 'System',
                [ModifiedDate] DATETIME NULL,
                [ModifiedBy] NVARCHAR(50) NULL,
                [Username] NVARCHAR(100) NOT NULL,
                [Email] NVARCHAR(100) NOT NULL,
                [PasswordHash] NVARCHAR(255) NOT NULL,
                [MustChangePassword] BIT NOT NULL CONSTRAINT [DF_Users_MustChangePassword] DEFAULT 0,
                [FullName] NVARCHAR(100) NOT NULL,
                [PhoneNumber] NVARCHAR(20) NULL,
                [SessionToken] NVARCHAR(255) NULL,
                [SessionExpiry] DATETIME NULL,
                [IsLocked] BIT NOT NULL CONSTRAINT [DF_Users_IsLocked] DEFAULT 0,
                [ResetToken] NVARCHAR(255) NULL,
                [ResetTokenExpiry] DATETIME NULL,
                [LastLoginDate] DATETIME NULL,
                [ProfilePhotoPath] NVARCHAR(1000) NULL,
                [ProfilePhotoFileName] NVARCHAR(255) NULL,
                CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
                CONSTRAINT [UQ_Users_Username] UNIQUE ([Username]),
                CONSTRAINT [UQ_Users_Email] UNIQUE ([Email])
            );
        END

        -- ============================================================
        -- CREATE INDEXES (menggunakan OBJECT_ID yang lebih aman)
        -- ============================================================
        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_IsActive' AND object_id = OBJECT_ID('Users'))
            CREATE INDEX [IX_Users_IsActive]
            ON [dbo].[Users]([IsActive])
            WHERE [IsActive] = 1;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
            CREATE INDEX [IX_Users_Email]
            ON [dbo].[Users]([Email]);

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_SessionToken' AND object_id = OBJECT_ID('Users'))
            CREATE INDEX [IX_Users_SessionToken]
            ON [dbo].[Users]([SessionToken])
            WHERE [SessionToken] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_ResetToken' AND object_id = OBJECT_ID('Users'))
            CREATE INDEX [IX_Users_ResetToken]
            ON [dbo].[Users]([ResetToken])
            WHERE [ResetToken] IS NOT NULL;

        IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_LastLoginDate' AND object_id = OBJECT_ID('Users'))
            CREATE INDEX [IX_Users_LastLoginDate]
            ON [dbo].[Users]([LastLoginDate] DESC);
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        -- ============================================================
        -- DROP INDEXES
        -- ============================================================
        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_LastLoginDate' AND object_id = OBJECT_ID('Users'))
            DROP INDEX [IX_Users_LastLoginDate] ON [dbo].[Users];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_ResetToken' AND object_id = OBJECT_ID('Users'))
            DROP INDEX [IX_Users_ResetToken] ON [dbo].[Users];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_SessionToken' AND object_id = OBJECT_ID('Users'))
            DROP INDEX [IX_Users_SessionToken] ON [dbo].[Users];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
            DROP INDEX [IX_Users_Email] ON [dbo].[Users];

        IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_IsActive' AND object_id = OBJECT_ID('Users'))
            DROP INDEX [IX_Users_IsActive] ON [dbo].[Users];

        -- ============================================================
        -- DROP TABLE
        -- ============================================================
        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
            DROP TABLE [dbo].[Users];
    ");
        }
    }
}