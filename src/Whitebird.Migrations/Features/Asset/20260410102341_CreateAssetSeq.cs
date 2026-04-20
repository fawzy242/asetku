using FluentMigrator;

namespace Whitebird.Migrations.Features.Asset
{
    [Migration(20260410102341)]
    public class CreateAssetSeq : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'Seq_AssetId' AND type = 'SO')
                BEGIN
                    CREATE SEQUENCE Seq_AssetId
                        START WITH 1
                        INCREMENT BY 1
                        MINVALUE 1
                        MAXVALUE 999999999
                        NO CYCLE
                        CACHE 10;
                END;
                GO
            ");
        }

        public override void Down()
        {
            Execute.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE name = 'Seq_AssetId' AND type = 'SO')
                    DROP SEQUENCE Seq_AssetId;
                GO
            ");
        }
    }
}
