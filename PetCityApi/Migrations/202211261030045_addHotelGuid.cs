namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addHotelGuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Hotels", "HotelGuid", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Hotels", "HotelGuid");
        }
    }
}
