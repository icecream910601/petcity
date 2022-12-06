namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addHotelTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Hotels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Account = c.String(),
                        Password = c.String(),
                        Identity = c.String(),
                        HotelName = c.String(),
                        HotelPhone = c.String(),
                        HotelAddress = c.String(),
                        HotelStartTime = c.String(),
                        HotelEndTime = c.String(),
                        HotelInfo = c.String(),
                        FoodTypes = c.String(),
                        ServiceTypes = c.String(),
                        AreaId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Areas", t => t.AreaId)
                .Index(t => t.AreaId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Hotels", "AreaId", "dbo.Areas");
            DropIndex("dbo.Hotels", new[] { "AreaId" });
            DropTable("dbo.Hotels");
        }
    }
}
