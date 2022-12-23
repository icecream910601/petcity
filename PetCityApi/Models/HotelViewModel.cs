using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetCityApi1.Models
{
    /// <summary>
    /// 畫面資料
    ///  使用者讀取旅館列表+分頁+篩選 (homepage) (前台)，不用帶TOKEN 
    /// </summary>
    public class HotelViewModel
    {
        /// <summary>
        /// Hotel資料清單
        /// </summary>
        public List<HotelData> Data { get; set; } //List 裡面放什麼型別都可以

        /// <summary>
        /// 總頁數
        /// </summary>
        public int Totalpage { get; set; }

        /// <summary>
        /// 目前第幾頁
        /// </summary>
        public int Nowpage { get; set; }
    }
}