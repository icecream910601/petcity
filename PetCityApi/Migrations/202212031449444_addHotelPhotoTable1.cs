namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addHotelPhotoTable1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HotelPhotoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Photo = c.String(),
                        HotelId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Hotels", t => t.HotelId)
                .Index(t => t.HotelId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.HotelPhotoes", "HotelId", "dbo.Hotels");
            DropIndex("dbo.HotelPhotoes", new[] { "HotelId" });
            DropTable("dbo.HotelPhotoes");
        }
    }
}
