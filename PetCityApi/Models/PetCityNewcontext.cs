using System;
using System.Data.Entity;
using System.Linq;

namespace PetCityApi1.Models
{
    public class PetCityNewcontext : DbContext
    {
        // 您的內容已設定為使用應用程式組態檔 (App.config 或 Web.config)
        // 中的 'PetCityNewcontext' 連接字串。根據預設，這個連接字串的目標是
        // 您的 LocalDb 執行個體上的 'PetCityApi1.Models.PetCityNewcontext' 資料庫。
        // 
        // 如果您的目標是其他資料庫和 (或) 提供者，請修改
        // 應用程式組態檔中的 'PetCityNewcontext' 連接字串。
        public PetCityNewcontext()
            : base("name=PetCityNewcontext")
        {
        }

        // 針對您要包含在模型中的每種實體類型新增 DbSet。如需有關設定和使用
        // Code First 模型的詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=390109。

        public virtual DbSet<Area> Areas { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Hotel> Hotels { get; set; }
        public virtual DbSet<KeepList> KeepLists { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<PetCard> PetCards { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<HotelPhoto> HotelPhotos { get; set; }


    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}