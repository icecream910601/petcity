namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class deleteReservedList : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.ReservedLists");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ReservedLists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Status = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id);
            
        }
    }
}
