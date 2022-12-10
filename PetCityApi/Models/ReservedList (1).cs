using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PetCityApi1.Models
{
    public class ReservedList
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //資料庫自動產生數字
        [Display(Name = "編號")]//欄位名稱
        public int Id { get; set; }  //屬性 習慣第一個字大寫 



        //[Required(ErrorMessage = "{0}必填")]
        [MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "入住狀況")]//欄位名稱
        public string Status { get; set; }


        public virtual ICollection<Order> Orders { get; set; } //virtual 虛擬的 //一個類別裡面有很多個消息
    }
}