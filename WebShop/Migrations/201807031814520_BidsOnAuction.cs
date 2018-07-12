namespace WebShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BidsOnAuction : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Auction", "State", c => c.String(maxLength: 10, unicode: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Auction", "State", c => c.String(nullable: false, maxLength: 10, unicode: false));
        }
    }
}
