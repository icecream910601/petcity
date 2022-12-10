using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace PetCityApi1.Models
{
    /// <summary>
    /// 旅館
    /// </summary>
    public class Hotel
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //資料庫自動產生數字
        [Display(Name = "編號")]//欄位名稱
        public int Id { get; set; }  //屬性 習慣第一個字大寫 



        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "旅館登入照片")]//欄位名稱
        public string HotelThumbnail { get; set; }


        [Required(ErrorMessage = "{0}必填")]     //
        [EmailAddress(ErrorMessage = "{0} 格式錯誤")]    //  
        [MaxLength(100)]
        [DataType(DataType.EmailAddress)]   //
        [Display(Name = "旅館帳號/Email")]//欄位名稱
        public string HotelAccount { get; set; }


        
        //[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])[a-zA-Z\d]{8,}$")]
        [Required(ErrorMessage = "{0}必填")]  //密碼長度至少為 Mini...
        [StringLength(100, ErrorMessage = "{0} 長度至少必須為 {2} 個字元。", MinimumLength = 8)] //
        [DataType(DataType.Password)]
        [Display(Name = "旅館密碼")]//欄位名稱
        public string HotelPassWord { get; set; }



        [Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "身分別")]//欄位名稱
        public string Identity { get; set; }


        
        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "旅館名稱")]//欄位名稱
        public string HotelName { get; set; }


        
        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "旅館電話")]//欄位名稱
        public string HotelPhone { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(100)]  //不設長度自動nvarchar(max)
        [Display(Name = "旅館地址")]//欄位名稱
        public string HotelAddress { get; set; }


        
        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "旅館營業開始時間")]//欄位名稱
        public string HotelStartTime { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "旅館營業結束時間")]//欄位名稱
        public string HotelEndTime { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        //[MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "旅館簡介")]//欄位名稱
        public string HotelInfo { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        //[MaxLength(50)]  //不設長度自動nvarchar(max)
        //[Display(Name = "旅館照片")]//欄位名稱
        //public string HotelPhoto { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "食物類型")]//欄位名稱
        public string FoodTypes { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        //[MaxLength(50)]  //不設長度自動nvarchar(max) 
        [Display(Name = "旅館服務")]//欄位名稱
        public string ServiceTypes { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        /*  [MaxLength(50)]*/  //不設長度自動nvarchar(max)
        [Display(Name = "旅館guid")]//欄位名稱
        public string HotelGuid { get; set; }


        
        [Display(Name = "地區")]
        public int? AreaId { get; set; }
        
        [ForeignKey("AreaId")]  //綁關聯   //透過ClassId 查出MyCatalog
        [JsonIgnore]
        public virtual Area Area { get; set; }//希望可以直接操縱所屬類別  //虛擬的  //我必須知道我的所屬類別是誰


        [JsonIgnore]
        public virtual ICollection<Room> Rooms { get; set; } //virtual 虛擬的 //一個類別裡面有很多個消息

        [JsonIgnore]
        public virtual ICollection<KeepList> KeepLists { get; set; } //virtual 虛擬的 //一個類別裡面有很多個消息

        [JsonIgnore]
        public virtual ICollection<HotelPhoto> HotelPhotos { get; set; } //virtual 虛擬的 //一個類別裡面有很多個消息

    }
}