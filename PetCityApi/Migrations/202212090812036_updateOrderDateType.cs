namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateOrderDateType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Orders", "OrderedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "OrderedDate", c => c.DateTime());
        }
    }
}
