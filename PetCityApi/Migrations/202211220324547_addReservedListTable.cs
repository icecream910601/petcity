namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addReservedListTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ReservedLists",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Orders", "ReservedListId", c => c.Int());
            CreateIndex("dbo.Orders", "ReservedListId");
            AddForeignKey("dbo.Orders", "ReservedListId", "dbo.ReservedLists", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Orders", "ReservedListId", "dbo.ReservedLists");
            DropIndex("dbo.Orders", new[] { "ReservedListId" });
            DropColumn("dbo.Orders", "ReservedListId");
            DropTable("dbo.ReservedLists");
        }
    }
}
