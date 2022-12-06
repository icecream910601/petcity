namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateCustomerTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "UserThumbnail", c => c.String(maxLength: 50));
            DropColumn("dbo.Customers", "UserPhoto");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Customers", "UserPhoto", c => c.String(maxLength: 50));
            DropColumn("dbo.Customers", "UserThumbnail");
        }
    }
}
