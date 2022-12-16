namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateRommPriceType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Rooms", "RoomPrice", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Rooms", "RoomPrice", c => c.String(maxLength: 50));
        }
    }
}
