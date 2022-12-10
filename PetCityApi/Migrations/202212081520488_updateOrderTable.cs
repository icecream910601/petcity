namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateOrderTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Orders", "ReservedListId", "dbo.ReservedLists");
            DropIndex("dbo.Orders", new[] { "ReservedListId" });
            AddColumn("dbo.Orders", "Status", c => c.String(maxLength: 50));
            DropColumn("dbo.Hotels", "HotelPhoto");
            DropColumn("dbo.Orders", "ReservedListId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Orders", "ReservedListId", c => c.Int());
            AddColumn("dbo.Hotels", "HotelPhoto", c => c.String());
            DropColumn("dbo.Orders", "Status");
            CreateIndex("dbo.Orders", "ReservedListId");
            AddForeignKey("dbo.Orders", "ReservedListId", "dbo.ReservedLists", "Id");
        }
    }
}
