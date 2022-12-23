using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetCityApi1.Models
{
    /// <summary>
    /// Hotel資料
    /// 使用者讀取旅館列表+分頁+篩選 (homepage) (前台)，不用帶TOKEN 
    /// </summary>
    public class HotelData
    {
        public int? RoomLowPrice { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; }
        public string HotelPhoto { get; set; }
        public double? HotelScore { get; set; }
        public string HotelInfo { get; set; }
    }
}