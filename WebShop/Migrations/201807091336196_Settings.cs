namespace WebShop.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Settings : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Setting",
                c => new
                    {
                        Key = c.String(nullable: false, maxLength: 128),
                        Value = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Key);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Setting");
        }
    }
}
