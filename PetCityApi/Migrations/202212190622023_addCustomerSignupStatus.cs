namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCustomerSignupStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "Status");
        }
    }
}
