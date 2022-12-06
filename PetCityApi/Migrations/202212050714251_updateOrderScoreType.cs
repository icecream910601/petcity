namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateOrderScoreType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Orders", "Score", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Orders", "Score", c => c.String(maxLength: 50));
        }
    }
}
