namespace WebShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TimestampTokenOrderCompletedAt2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TokenOrders", "CompletedAt", c => c.DateTime(nullable: true));

        }

        public override void Down()
        {
            DropColumn("dbo.TokenOrders", "CompletedAt");

        }
    }
}
