namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateOrderContactInfo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Orders", "UserName", c => c.String(maxLength: 50));
            AddColumn("dbo.Orders", "UserPhone", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Orders", "UserPhone");
            DropColumn("dbo.Orders", "UserName");
        }
    }
}
