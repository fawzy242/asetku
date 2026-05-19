using FluentMigrator;

namespace Whitebird.Migrations.Features.User
{
    [Migration(20260519213352)]
    public class CreateTableUser : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
        IF NOT EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'Users' AND [xtype] = 'U')
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
                [FullName] NVARCHAR(100) NOT NULL,
                [PhoneNumber] NVARCHAR(20) NULL,
                [SessionToken] NVARCHAR(255) NULL,
                [SessionExpiry] DATETIME NULL,
                [IsLocked] BIT NOT NULL CONSTRAINT [DF_Users_IsLocked] DEFAULT 0,
                [ResetToken] NVARCHAR(255) NULL,
                [ResetTokenExpiry] DATETIME NULL,
                [LastLoginDate] DATETIME NULL,
                CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
                CONSTRAINT [UQ_Users_Username] UNIQUE ([Username]),
                CONSTRAINT [UQ_Users_Email] UNIQUE ([Email])
            );
        END;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Users_IsActive')
            CREATE INDEX [IX_Users_IsActive]
            ON [dbo].[Users]([IsActive])
            WHERE [IsActive] = 1;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Users_Email')
            CREATE INDEX [IX_Users_Email]
            ON [dbo].[Users]([Email]);

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Users_SessionToken')
            CREATE INDEX [IX_Users_SessionToken]
            ON [dbo].[Users]([SessionToken])
            WHERE [SessionToken] IS NOT NULL;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Users_ResetToken')
            CREATE INDEX [IX_Users_ResetToken]
            ON [dbo].[Users]([ResetToken])
            WHERE [ResetToken] IS NOT NULL;

        IF NOT EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Users_LastLoginDate')
            CREATE INDEX [IX_Users_LastLoginDate]
            ON [dbo].[Users]([LastLoginDate] DESC);
    ");
        }

        public override void Down()
        {
            Execute.Sql(@"
        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Users_LastLoginDate')
            DROP INDEX [IX_Users_LastLoginDate] ON [dbo].[Users];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Users_ResetToken')
            DROP INDEX [IX_Users_ResetToken] ON [dbo].[Users];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Users_SessionToken')
            DROP INDEX [IX_Users_SessionToken] ON [dbo].[Users];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Users_Email')
            DROP INDEX [IX_Users_Email] ON [dbo].[Users];

        IF EXISTS (SELECT * FROM [sys.indexes] WHERE [name] = 'IX_Users_IsActive')
            DROP INDEX [IX_Users_IsActive] ON [dbo].[Users];

        IF EXISTS (SELECT * FROM [sysobjects] WHERE [name] = 'Users' AND [xtype] = 'U')
            DROP TABLE [dbo].[Users];
    ");
        }
    }
}
