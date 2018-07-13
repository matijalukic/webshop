namespace WebShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TokenOrdersPriceDecimal : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Auction", "Picture", c => c.String(unicode: false, storeType: "text"));
            AlterColumn("dbo.TokenOrders", "Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TokenOrders", "Price", c => c.Int(nullable: false));
            AlterColumn("dbo.Auction", "Picture", c => c.String(nullable: false, unicode: false, storeType: "text"));
        }
    }
}
