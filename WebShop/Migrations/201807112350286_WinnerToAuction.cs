namespace WebShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WinnerToAuction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Auction", "WinnerId", c => c.Int());
            CreateIndex("dbo.Auction", "WinnerId");
            AddForeignKey("dbo.Auction", "WinnerId", "dbo.Users", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Auction", "WinnerId", "dbo.Users");
            DropIndex("dbo.Auction", new[] { "WinnerId" });
            DropColumn("dbo.Auction", "WinnerId");
        }
    }
}
