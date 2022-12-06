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
            [MaxLength(50)]  //不設長度自動nvarchar(max)
            [Display(Name = "使用者姓名")]//欄位名稱
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


    }
}