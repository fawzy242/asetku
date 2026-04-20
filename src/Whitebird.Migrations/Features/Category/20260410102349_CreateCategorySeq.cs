using FluentMigrator;

namespace Whitebird.Migrations.Features.Category
{
    [Migration(20260410102349)]
    public class CreateCategorySeq : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'Seq_CategoryId' AND type = 'SO')
                BEGIN
                    CREATE SEQUENCE Seq_CategoryId
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
                IF EXISTS (SELECT * FROM sys.objects WHERE name = 'Seq_CategoryId' AND type = 'SO')
                    DROP SEQUENCE Seq_CategoryId;
                GO
            ");
        }
    }
}
