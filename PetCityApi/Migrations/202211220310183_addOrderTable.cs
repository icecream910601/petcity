namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addOrderTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrderedDate = c.DateTime(),
                        CheckInDate = c.DateTime(),
                        CheckOutDate = c.DateTime(),
                        Score = c.String(),
                        Comment = c.String(),
                        PetCardId = c.Int(),
                        RoomId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PetCards", t => t.PetCardId)
                .ForeignKey("dbo.Rooms", t => t.RoomId)
                .Index(t => t.PetCardId)
                .Index(t => t.RoomId);
            
            CreateTable(
                "dbo.PetCards",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PetPhoto = c.String(),
                        PetName = c.String(),
                        PetType = c.String(),
                        PetAge = c.String(),
                        PetSex = c.String(),
                        FoodTypes = c.String(),
                        PetPersonality = c.String(),
                        PetMedicine = c.String(),
                        PetNote = c.String(),
                        ServiceTypes = c.String(),
                        CustomerId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customers", t => t.CustomerId)
                .Index(t => t.CustomerId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Orders", "RoomId", "dbo.Rooms");
            DropForeignKey("dbo.Orders", "PetCardId", "dbo.PetCards");
            DropForeignKey("dbo.PetCards", "CustomerId", "dbo.Customers");
            DropIndex("dbo.PetCards", new[] { "CustomerId" });
            DropIndex("dbo.Orders", new[] { "RoomId" });
            DropIndex("dbo.Orders", new[] { "PetCardId" });
            DropTable("dbo.PetCards");
            DropTable("dbo.Orders");
        }
    }
}
