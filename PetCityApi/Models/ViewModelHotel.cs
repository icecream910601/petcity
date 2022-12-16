using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PetCityApi1.Models
{
    public class ViewModelHotel
    {
        /// <summary>
        /// 旅館註冊
        /// </summary>
        public class Signup
        {
            [Required(ErrorMessage = "{0}必填")] //
            [EmailAddress(ErrorMessage = "{0} 格式錯誤")] //  
            [MaxLength(100)]
            [DataType(DataType.EmailAddress)] //
            [Display(Name = "旅館帳號/Email")] //欄位名稱
            public string HotelAccount { get; set; }



            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "旅館姓名")] //欄位名稱
            public string HotelName { get; set; }



            //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])[a-zA-Z\d]{8,}$")]
            [Required(ErrorMessage = "{0}必填")] //長度   密碼長度至少為 mini
            [StringLength(100, ErrorMessage = "{0} 長度至少必須為 {2} 個字元。", MinimumLength = 8)] //
            [DataType(DataType.Password)]
            [Display(Name = "旅館密碼")] //欄位名稱
            public string HotelPassWord { get; set; }



            [Required(ErrorMessage = "{0}必填")]
            [DataType(DataType.Password)]
            [Display(Name = "再次確認密碼")]
            [Compare("HotelPassWord", ErrorMessage = "密碼不一致.")]
            public string ConfirmedPassword { get; set; }




            [Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "身分別")] //欄位名稱
            public string Identity { get; set; }
        }

        /// <summary>
        /// 旅館登入
        /// </summary>
        public class Login //: Signup  //繼承
        {
            [Required(ErrorMessage = "{0}必填")] //
            [EmailAddress(ErrorMessage = "{0} 格式錯誤")] //  
            [MaxLength(100)]
            [DataType(DataType.EmailAddress)] //
            [Display(Name = "旅館帳號/Email")] //欄位名稱
            public string HotelAccount { get; set; }


            //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])[a-zA-Z\d]{8,}$")]
            [Required(ErrorMessage = "{0}必填")] //長度   密碼長度至少為 mini
            [StringLength(100, ErrorMessage = "{0} 長度至少必須為 {2} 個字元。", MinimumLength = 8)] //
            [DataType(DataType.Password)]
            [Display(Name = "旅館密碼")] //欄位名稱
            public string HotelPassWord { get; set; }



            [Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "身分別")] //欄位名稱
            public string Identity { get; set; }


        }


        /// <summary>
        /// 旅館忘記密碼寄信
        /// </summary>
        public class ForgetPassword
        {
            [Required(ErrorMessage = "{0}必填")] //
            [EmailAddress(ErrorMessage = "{0} 格式錯誤")] //  
            [MaxLength(100)]
            [DataType(DataType.EmailAddress)] //
            [Display(Name = "旅館帳號/Email")] //欄位名稱
            public string HotelAccount { get; set; }

        }

        /// <summary>
        /// 旅館重設密碼 
        /// </summary>
        public class ResetPassword
        {

            [Required(ErrorMessage = "{0}必填")]
            [StringLength(100, ErrorMessage = "{0} 長度至少必須為 {2} 個字元。", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "新密碼")]
            public string NewPassword { get; set; }


            [Required(ErrorMessage = "{0}必填")]
            [StringLength(100, ErrorMessage = "{0} 長度至少必須為 {2} 個字元。", MinimumLength = 8)]
            [DataType(DataType.Password)]
            [Display(Name = "再次確認密碼")]
            [Compare("NewPassword", ErrorMessage = "密碼不一致.")]
            public string ConfirmedPassword { get; set; }

        }


        /// <summary>
        /// 旅館修改資料
        /// </summary>
        public class ModifyHotel
        {
            ////[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)] //不設長度自動nvarchar(max)
            //[Display(Name = "旅館登入照片")] //欄位名稱
            //public string HotelThumbnail { get; set; }



            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "旅館名稱")] //欄位名稱
            public string HotelName { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "旅館電話")] //欄位名稱
            public string HotelPhone { get; set; }



            [Display(Name = "地區")] public int? AreaId { get; set; }

            //[ForeignKey("AreaId")]  //綁關聯   //透過ClassId 查出MyCatalog
            //public virtual Area Area { get; set; }//希望可以直接操縱所屬類別  //虛擬的  //我必須知道我的所屬類別是誰



            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(100)] //不設長度自動nvarchar(max)
            [Display(Name = "旅館地址")] //欄位名稱
            public string HotelAddress { get; set; }


            ////[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)] //不設長度自動nvarchar(max)
            //[Display(Name = "旅館營業開始時間")] //欄位名稱
            //public string HotelStartTime { get; set; }



            ////[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)] //不設長度自動nvarchar(max)
            //[Display(Name = "旅館營業結束時間")] //欄位名稱
            //public string HotelEndTime { get; set; }




            //[Required(ErrorMessage = "{0}必填")]
            /*[MaxLength(50)]*/ //不設長度自動nvarchar(max)
            [Display(Name = "旅館營業時間")] //欄位名稱
            public List<string> HotelBusinessTime { get; set; }



            //[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "旅館簡介")] //欄位名稱
            public string HotelInfo { get; set; }



            ////[Required(ErrorMessage = "{0}必填")]
            ////[MaxLength(50)]  //不設長度自動nvarchar(max)
            //[Display(Name = "旅館照片")] //欄位名稱
            //public string HotelPhoto { get; set; }



            //[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)] 不設長度自動nvarchar(max)
            [Display(Name = "食物類型")] //欄位名稱
            public List<string> FoodTypes { get; set; }



            //[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)]  //不設長度自動nvarchar(max) 
            [Display(Name = "旅館服務")] //欄位名稱
            public List<string> ServiceTypes { get; set; }
        }

        /// <summary>
        /// 房型修改資料
        /// </summary>
        public class Room
        {
            //[Display(Name = "旅館")]
            //public int? HotelId { get; set; }

            ////[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)]  //不設長度自動nvarchar(max)
            //[Display(Name = "房型照片")]//欄位名稱
            //public string RoomPhoto { get; set; }

            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "房型名稱")]//欄位名稱
            public string RoomName { get; set; }



            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "寵物類型")]//欄位名稱
            public string PetType { get; set; }



            //[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "房型價格")]//欄位名稱
            public int? RoomPrice { get; set; }



            //[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "房間簡介")]//欄位名稱
            public string RoomInfo { get; set; }
        }

        public class Filter
        {
            [Display(Name = "地區")] public int? AreaId { get; set; }

            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "寵物類型")]//欄位名稱
            public string PetType { get; set; }



            //[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)] 不設長度自動nvarchar(max)
            [Display(Name = "食物類型")] //欄位名稱
            public string[] FoodTypes { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)]  //不設長度自動nvarchar(max) 
            [Display(Name = "旅館服務")] //欄位名稱
            public string[] ServiceTypes { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "入住日期")]//欄位名稱
            public string CheckInDate { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "退房日期")]//欄位名稱
            public string CheckOutDate { get; set; }


            ////[Required(ErrorMessage = "{0}必填")]
            ////[MaxLength(50)]  //不設長度自動nvarchar(max)
            //[Display(Name = "房型起始價格")]//欄位名稱
            //public int RoomPriceStart { get; set; }

            ////[Required(ErrorMessage = "{0}必填")]
            ////[MaxLength(50)]  //不設長度自動nvarchar(max)
            //[Display(Name = "房型結束價格")]//欄位名稱
            //public int RoomPriceEnd { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)]  //不設長度自動nvarchar(max) 
            [Display(Name = "價格區間")] //欄位名稱
            public int[] PriceRange { get; set; }


            
            public int Page { get; set; }
            public int PageSize { get; set; }
        }





        public class CheckInStatus
        {
            public string CheckIn { get; set; }
        }
        public class CheckOutStatus
        {
            public string CheckOut { get; set; }
        }
        public class CancelStatus
        {
            public string Cancel{ get; set; }
        }

    }
}