namespace WebShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Auction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50, unicode: false),
                        Picture = c.String(nullable: false, unicode: false, storeType: "text"),
                        Duration = c.Int(nullable: false),
                        StartPrice = c.Int(nullable: false),
                        Price = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ClosedAt = c.DateTime(),
                        State = c.String(nullable: false, maxLength: 10, unicode: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        Surname = c.String(maxLength: 50, unicode: false),
                        Email = c.String(maxLength: 50, unicode: false),
                        Password = c.String(maxLength: 100, unicode: false),
                        Tokens = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Bids",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Amount = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        AuctionId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Auction", t => t.AuctionId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: false)
                .Index(t => t.AuctionId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.TokenOrders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.Int(nullable: false),
                        Amount = c.Int(nullable: false),
                        Price = c.Int(nullable: false),
                        State = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TokenOrders", "UserId", "dbo.Users");
            DropForeignKey("dbo.Bids", "UserId", "dbo.Users");
            DropForeignKey("dbo.Bids", "AuctionId", "dbo.Auction");
            DropForeignKey("dbo.Auction", "UserId", "dbo.Users");
            DropIndex("dbo.TokenOrders", new[] { "UserId" });
            DropIndex("dbo.Bids", new[] { "UserId" });
            DropIndex("dbo.Bids", new[] { "AuctionId" });
            DropIndex("dbo.Auction", new[] { "UserId" });
            DropTable("dbo.TokenOrders");
            DropTable("dbo.Bids");
            DropTable("dbo.Users");
            DropTable("dbo.Auction");
        }
    }
}
