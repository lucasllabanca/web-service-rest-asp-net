namespace TrabalhoPraticoDM106.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixingNullableAndNotNullableFields : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Orders", "Data", c => c.DateTime());
            AlterColumn("dbo.Orders", "DataEntrega", c => c.DateTime());
            AlterColumn("dbo.Products", "Altura", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.Products", "Largura", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.Products", "Comprimento", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.Products", "Diametro", c => c.Decimal(precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Products", "Diametro", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Products", "Comprimento", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Products", "Largura", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Products", "Altura", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AlterColumn("dbo.Orders", "DataEntrega", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Orders", "Data", c => c.DateTime(nullable: false));
        }
    }
}
