namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatePhotoLength : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Hotels", "HotelThumbnail", c => c.String(maxLength: 100));
            AlterColumn("dbo.Customers", "UserThumbnail", c => c.String(maxLength: 100));
            AlterColumn("dbo.Rooms", "RoomPhoto", c => c.String(maxLength: 100));
            AlterColumn("dbo.PetCards", "PetPhoto", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PetCards", "PetPhoto", c => c.String(maxLength: 50));
            AlterColumn("dbo.Rooms", "RoomPhoto", c => c.String(maxLength: 50));
            AlterColumn("dbo.Customers", "UserThumbnail", c => c.String(maxLength: 50));
            AlterColumn("dbo.Hotels", "HotelThumbnail", c => c.String(maxLength: 50));
        }
    }
}
