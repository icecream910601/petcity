namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCustomerGuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Customers", "UserGuid", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Customers", "UserGuid");
        }
    }
}
