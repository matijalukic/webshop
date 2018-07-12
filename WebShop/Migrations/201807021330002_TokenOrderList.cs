namespace WebShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TokenOrderList : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Users", new[] { "Email" });
            AlterColumn("dbo.Users", "Name", c => c.String(nullable: false, unicode: false));
            AlterColumn("dbo.Users", "Surname", c => c.String(nullable: false, maxLength: 50, unicode: false));
            AlterColumn("dbo.Users", "Email", c => c.String(nullable: false, maxLength: 50, unicode: false));
            CreateIndex("dbo.Users", "Email", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Users", new[] { "Email" });
            AlterColumn("dbo.Users", "Email", c => c.String(maxLength: 50, unicode: false));
            AlterColumn("dbo.Users", "Surname", c => c.String(maxLength: 50, unicode: false));
            AlterColumn("dbo.Users", "Name", c => c.String(unicode: false));
            CreateIndex("dbo.Users", "Email", unique: true);
        }
    }
}
