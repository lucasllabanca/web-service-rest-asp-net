namespace TrabalhoPraticoDM106.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixingNullableOrderUserEmail : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Orders", "UserEmail", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "UserEmail", c => c.String(nullable: false));
        }
    }
}
