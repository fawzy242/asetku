using FluentMigrator;

namespace Whitebird.Migrations.Features.Supplier
{
    [Migration(20260410102347)]
    public class CreateSupplierSeq : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'Seq_SupplierId' AND type = 'SO')
                BEGIN
                    CREATE SEQUENCE Seq_SupplierId
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
                IF EXISTS (SELECT * FROM sys.objects WHERE name = 'Seq_SupplierId' AND type = 'SO')
                    DROP SEQUENCE Seq_SupplierId;
                GO
            ");
        }
    }
}
