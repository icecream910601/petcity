namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateHotelTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.HotelPhotoes", "HotelId", "dbo.Hotels");
            DropIndex("dbo.HotelPhotoes", new[] { "HotelId" });
            AddColumn("dbo.Hotels", "HotelThumbnail", c => c.String(maxLength: 50));
            DropTable("dbo.HotelPhotoes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.HotelPhotoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        HotelPhotos = c.String(maxLength: 50),
                        HotelId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.Hotels", "HotelThumbnail");
            CreateIndex("dbo.HotelPhotoes", "HotelId");
            AddForeignKey("dbo.HotelPhotoes", "HotelId", "dbo.Hotels", "Id");
        }
    }
}
