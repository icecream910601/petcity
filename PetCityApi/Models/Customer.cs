using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PetCityApi1.Models
{
    /// <summary>
    /// 顧客
    /// </summary>
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //資料庫自動產生數字
        [Display(Name = "編號")]//欄位名稱
        public int Id { get; set; }  //屬性 習慣第一個字大寫 



        [Required(ErrorMessage = "{0}必填")]     //
        [EmailAddress(ErrorMessage = "{0} 格式錯誤")]    //  
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]   //
        [Display(Name = "使用者帳號/Email")]//欄位名稱
        public string UserAccount { get; set; }


        
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])[a-zA-Z\d]{8,}$")]
        [Required(ErrorMessage = "{0}必填")]  //長度   密碼長度至少為 4
        [StringLength(100, ErrorMessage = "{0} 長度至少必須為 {2} 個字元。", MinimumLength = 8)] //
        [DataType(DataType.Password)]
        [Display(Name = "使用者密碼")]//欄位名稱
        public string UserPassWord { get; set; }



        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "身分別")]//欄位名稱
        public string Identity { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "使用者姓名")]//欄位名稱
        public string UserName { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "使用者電話")]//欄位名稱
        public string UserPhone { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(100)]  //不設長度自動nvarchar(max)
        [Display(Name = "使用者地址")]//欄位名稱
        public string UserAddress { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(100)]  //不設長度自動nvarchar(max)
        [Display(Name = "會員登入照片")]//欄位名稱
        public string UserThumbnail { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
      /*  [MaxLength(50)]*/  //不設長度自動nvarchar(max)
        [Display(Name = "會員guid")]//欄位名稱
        public string UserGuid { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        /*  [MaxLength(50)]*/  //不設長度自動nvarchar(max)
        [Display(Name = "開通狀態")]//欄位名稱
        public string Status { get; set; }



        public virtual ICollection<KeepList> KeepLists { get; set; } //virtual 虛擬的 //一個類別裡面有很多個消息
    }
}