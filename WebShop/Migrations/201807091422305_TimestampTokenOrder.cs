namespace WebShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TimestampTokenOrder : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TokenOrders", "CreatedAt", c => c.DateTime(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TokenOrders", "CreatedAt");
        }
    }
}
