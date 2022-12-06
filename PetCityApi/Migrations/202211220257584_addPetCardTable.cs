namespace PetCityApi1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPetCardTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Rooms", "PetType", c => c.String());
            DropColumn("dbo.Rooms", "PetTypes");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Rooms", "PetTypes", c => c.String());
            DropColumn("dbo.Rooms", "PetType");
        }
    }
}
