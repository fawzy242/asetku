using FluentMigrator;

namespace Whitebird.Migrations.Features.User
{
    [Migration(20260521001402)]
    public class AlterTableAddProfileImage : Migration
    {
            public override void Up()
    {
        Execute.Sql(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'ProfilePhotoPath')
            BEGIN
                ALTER TABLE [dbo].[Users] ADD 
                    [ProfilePhotoPath] NVARCHAR(1000) NULL,
                    [ProfilePhotoFileName] NVARCHAR(255) NULL
            END
        ");
    }

    public override void Down()
    {
        Execute.Sql(@"
            IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'ProfilePhotoPath')
            BEGIN
                ALTER TABLE [dbo].[Users] DROP COLUMN [ProfilePhotoPath], [ProfilePhotoFileName]
            END
        ");
    }
    }
}
