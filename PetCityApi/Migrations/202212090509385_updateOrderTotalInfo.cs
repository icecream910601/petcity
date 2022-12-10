namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateOrderTotalInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "TotalNight", c => c.Int());
            AddColumn("dbo.Orders", "TotalPrice", c => c.Int());
            AlterColumn("dbo.Orders", "OrderedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "OrderedDate", c => c.DateTime());
            DropColumn("dbo.Orders", "TotalPrice");
            DropColumn("dbo.Orders", "TotalNight");
        }
    }
}
