namespace TrabalhoPraticoDM106.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProduct : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nome = c.String(nullable: false),
                        Descricao = c.String(),
                        Cor = c.String(),
                        Modelo = c.String(nullable: false),
                        Codigo = c.String(nullable: false),
                        Preco = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Peso = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Altura = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Largura = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Comprimento = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Diametro = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Url = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Products");
        }
    }
}
