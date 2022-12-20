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
    /// 寵物名片
    /// </summary>
    public class PetCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //資料庫自動產生數字
        [Display(Name = "編號")] //欄位名稱
        public int Id { get; set; } //屬性 習慣第一個字大寫 



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
        [Display(Name = "寵物類型")] //欄位名稱
        public string PetType { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)] //不設長度自動nvarchar(max)
        [Display(Name = "寵物年齡")] //欄位名稱
        public string PetAge { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)] //不設長度自動nvarchar(max)
        [Display(Name = "寵物性別")] //欄位名稱
        public string PetSex { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)] //不設長度自動nvarchar(max)
        [Display(Name = "食物類型")] //欄位名稱
        public string FoodTypes { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        //[MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "寵物個性")] //欄位名稱
        public string PetPersonality { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        //[MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "寵物服用藥物")] //欄位名稱
        public string PetMedicine { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        //[MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "備註")] //欄位名稱
        public string PetNote { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        //[MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "旅館服務")] //欄位名稱
        public string ServiceTypes { get; set; }


        [JsonIgnore]
        [Display(Name = "飼主")] public int? CustomerId { get; set; }

        [ForeignKey("CustomerId")] //綁關聯   //透過ClassId 查出MyCatalog
        public virtual Customer Customer { get; set; } //希望可以直接操縱所屬類別  //虛擬的  //我必須知道我的所屬類別是誰


        [JsonIgnore]
        public virtual ICollection<Order> Orders { get; set; } //virtual 虛擬的 //一個類別裡面有很多個消息


    }
}