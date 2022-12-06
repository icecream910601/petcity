namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addKeepListTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.KeepLists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(),
                        HotelId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customers", t => t.CustomerId)
                .ForeignKey("dbo.Hotels", t => t.HotelId)
                .Index(t => t.CustomerId)
                .Index(t => t.HotelId);
            
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserAccount = c.String(),
                        UserPassWord = c.String(),
                        Identity = c.String(),
                        UserName = c.String(),
                        UserPhone = c.String(),
                        UserAddress = c.String(),
                        UserPhoto = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Hotels", "HotelPhoto", c => c.String());
            AddColumn("dbo.Hotels", "HotelAccount", c => c.String());
            AddColumn("dbo.Hotels", "HotelPassword", c => c.String());
            DropColumn("dbo.Hotels", "Account");
            DropColumn("dbo.Hotels", "Password");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Hotels", "Password", c => c.String());
            AddColumn("dbo.Hotels", "Account", c => c.String());
            DropForeignKey("dbo.KeepLists", "HotelId", "dbo.Hotels");
            DropForeignKey("dbo.KeepLists", "CustomerId", "dbo.Customers");
            DropIndex("dbo.KeepLists", new[] { "HotelId" });
            DropIndex("dbo.KeepLists", new[] { "CustomerId" });
            DropColumn("dbo.Hotels", "HotelPassword");
            DropColumn("dbo.Hotels", "HotelAccount");
            DropColumn("dbo.Hotels", "HotelPhoto");
            DropTable("dbo.Customers");
            DropTable("dbo.KeepLists");
        }
    }
}
