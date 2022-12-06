using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace PetCityApi1.Models
{
    public class KeepList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //資料庫自動產生數字
        [Display(Name = "編號")]//欄位名稱
        public int Id { get; set; }  //屬性 習慣第一個字大寫 


        
        [Display(Name = "飼主")]
        public int? CustomerId { get; set; }
        
        [ForeignKey("CustomerId")]  //綁關聯   //透過ClassId 查出MyCatalog
        public virtual Customer Customers { get; set; }//希望可以直接操縱所屬類別  //虛擬的  //我必須知道我的所屬類別是誰



        [Display(Name = "旅館")]
        public int? HotelId { get; set; }

        [ForeignKey("HotelId")]  //綁關聯   //透過ClassId 查出MyCatalog
        [JsonIgnore]
        public virtual Hotel Hotel { get; set; }//希望可以直接操縱所屬類別  //虛擬的  //我必須知道我的所屬類別是誰

    }
}