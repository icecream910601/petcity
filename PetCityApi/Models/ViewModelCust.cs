using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PetCityApi1.Models
{
    public class ViewModelCust
    {
        /// <summary>
        /// 會員註冊
        /// </summary>
        public class Signup
        {
            [Required(ErrorMessage = "{0}必填")] //
            [EmailAddress(ErrorMessage = "{0} 格式錯誤")] //  
            [MaxLength(100)]
            [DataType(DataType.EmailAddress)] //
            [Display(Name = "使用者帳號/Email")] //欄位名稱
            public string UserAccount { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "使用者姓名")] //欄位名稱
            public string UserName { get; set; }


            //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])[a-zA-Z\d]{8,}$")]
            [Required(ErrorMessage = "{0}必填")] //長度   密碼長度至少為 mini
            [StringLength(100, ErrorMessage = "{0} 長度至少必須為 {2} 個字元。", MinimumLength = 8)] //
            [DataType(DataType.Password)]
            [Display(Name = "使用者密碼")] //欄位名稱
            public string UserPassWord { get; set; }


            [Required(ErrorMessage = "{0}必填")]
            [DataType(DataType.Password)]
            [Display(Name = "再次確認密碼")]
            [Compare("UserPassWord", ErrorMessage = "密碼不一致.")]
            public string ConfirmedPassword { get; set; }


            [Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "身分別")] //欄位名稱
            public string Identity { get; set; }
        }

        /// <summary>
        /// 會員登入
        /// </summary>
        public class Login
        {
            [Required(ErrorMessage = "{0}必填")] //
            [EmailAddress(ErrorMessage = "{0} 格式錯誤")] //  
            [MaxLength(100)]
            [DataType(DataType.EmailAddress)] //
            [Display(Name = "使用者帳號/Email")] //欄位名稱
            public string UserAccount { get; set; }


            //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])[a-zA-Z\d]{8,}$")]
            [Required(ErrorMessage = "{0}必填")] //長度   密碼長度至少為 mini
            [StringLength(100, ErrorMessage = "{0} 長度至少必須為 {2} 個字元。", MinimumLength = 8)] //
            [DataType(DataType.Password)]
            [Display(Name = "使用者密碼")] //欄位名稱
            public string UserPassWord { get; set; }
        }

        /// <summary>
        /// 會員忘記密碼寄信
        /// </summary>
        public class ForgetPassword
        {
            [Required(ErrorMessage = "{0}必填")] //
            [EmailAddress(ErrorMessage = "{0} 格式錯誤")] //  
            [MaxLength(100)]
            [DataType(DataType.EmailAddress)] //
            [Display(Name = "使用者帳號/Email")] //欄位名稱
            public string UserAccount { get; set; }
        }


        /// <summary>
        /// 會員重設密碼 
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
        /// 會員資料
        /// </summary>
        public class UserInfo
        {
            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "使用者姓名")] //欄位名稱
            public string UserName { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "使用者電話")] //欄位名稱
            public string UserPhone { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(100)] //不設長度自動nvarchar(max)
            [Display(Name = "使用者地址")] //欄位名稱
            public string UserAddress { get; set; }
        }

        /// <summary>
        /// 會員送出訂單
        /// </summary>
        public class Book
        {
            [Display(Name = "寵物卡編號")] //欄位名稱
            public int PetCardId { get; set; } //屬性 習慣第一個字大寫


            [Display(Name = "房間編號")] //欄位名稱
            public int RoomId { get; set; } //屬性 習慣第一個字大寫


            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            [Display(Name = "訂單日期")]
            [DisplayFormat(ApplyFormatInEditMode = true,
                DataFormatString = "{0:yyyy-MM-dd}")] // 資料庫雖然看不到時間 //但其實有期間  //不要顯示時分秒 //0:d 0000/00/00 0000-00-00
            [DataType(DataType.Date)]
            public DateTime? OrderedDate { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "入住日期")] //欄位名稱
            public string CheckInDate { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "退房日期")] //欄位名稱
            public string CheckOutDate { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "總共入住幾晚")] //欄位名稱
            public int? TotalNight { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "訂單總價格")] //欄位名稱
            public int? TotalPrice { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "使用者姓名")] //欄位名稱
            public string UserName { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "使用者電話")] //欄位名稱
            public string UserPhone { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "訂單狀況")] //欄位名稱
            public string Status { get; set; }
        }

        /// <summary>
        /// 會員查看訂單
        /// </summary>
        public class OrderList
        {

            [Display(Name = "訂單編號")] //欄位名稱
            public int OrderId { get; set; } //屬性 習慣第一個 字大寫 


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "房型照片")] //欄位名稱
            public string RoomPhoto { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "旅館名稱")] //欄位名稱
            public string HotelName { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "房型名稱")] //欄位名稱
            public string RoomName { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "入住日期")]//欄位名稱
            public string CheckInDate { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "退房日期")]//欄位名稱
            public string CheckOutDate { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "訂單總價格")]//欄位名稱
            public int? TotalPrice { get; set; }


            [Display(Name = "寵物卡編號")] //欄位名稱
            public int? PetCardId { get; set; } //屬性 習慣第一個 字大寫 


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(100)] //不設長度自動nvarchar(max)
            [Display(Name = "寵物照片")] //欄位名稱
            public string PetPhoto { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "寵物名字")] //欄位名稱
            public string PetName { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "訂單狀況")] //欄位名稱
            public string Status { get; set; }

        }

        /// <summary>
        /// 會員查看評論
        /// </summary>

        public class CommentList 
        {

            [Display(Name = "編號")] //欄位名稱
            public int Id { get; set; } //屬性 習慣第一個字大寫 


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "房型照片")] //欄位名稱
            public string RoomPhoto { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "旅館名稱")] //欄位名稱
            public string HotelName { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "房型名稱")] //欄位名稱
            public string RoomName { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "入住日期")]//欄位名稱
            public string CheckInDate { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "退房日期")]//欄位名稱
            public string CheckOutDate { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "訂單狀況")] //欄位名稱
            public string Status { get; set; }

        }

        /// <summary>
        /// 會員送出評論
        /// </summary>
        public class SendCommentModel
        {
            //[Required(ErrorMessage = "{0}必填")]
            /*[MaxLength(50)] */ //不設長度自動nvarchar(max)
            [Display(Name = "安心度")] //欄位名稱
            public int? Score { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            //[MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "評論")] //欄位名稱
            public string Comment { get; set; }


            //[Required(ErrorMessage = "{0}必填")]
            [MaxLength(50)] //不設長度自動nvarchar(max)
            [Display(Name = "訂單狀況")] //欄位名稱
            public string Status { get; set; }
        }

    }
}