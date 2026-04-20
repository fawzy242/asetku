using FluentMigrator;

namespace Whitebird.Migrations.Features.Location
{
    [Migration(20260410102353)]
    public class CreateLocationSeq : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'Seq_LocationId' AND type = 'SO')
                BEGIN
                    CREATE SEQUENCE Seq_LocationId
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
                IF EXISTS (SELECT * FROM sys.objects WHERE name = 'Seq_LocationId' AND type = 'SO')
                    DROP SEQUENCE Seq_LocationId;
                GO
            ");
        }
    }
}
