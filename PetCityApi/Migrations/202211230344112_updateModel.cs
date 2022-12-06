namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateModel : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Hotels", "HotelAccount", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Customers", "UserAccount", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Customers", "UserAccount", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.Hotels", "HotelAccount", c => c.String(nullable: false, maxLength: 200));
        }
    }
}
