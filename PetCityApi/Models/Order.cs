using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PetCityApi1.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] //資料庫自動產生數字
        [Display(Name = "編號")]//欄位名稱
        public int Id { get; set; }  //屬性 習慣第一個字大寫 


         
        [Display(Name = "訂單日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]  // 資料庫雖然看不到時間 //但其實有期間  //不要顯示時分秒 //0:d 0000/00/00 0000-00-00
        [DataType(DataType.Date)]
        public DateTime? OrderedDate { get; set; }



        [Display(Name = "入住日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]  // 資料庫雖然看不到時間 //但其實有期間  //不要顯示時分秒 //0:d 0000/00/00 0000-00-00
        [DataType(DataType.Date)]
        public DateTime? CheckInDate { get; set; }


        
        [Display(Name = "退房日期")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]  // 資料庫雖然看不到時間 //但其實有期間  //不要顯示時分秒 //0:d 0000/00/00 0000-00-00
        [DataType(DataType.Date)]
        public DateTime? CheckOutDate { get; set; }



        //[Required(ErrorMessage = "{0}必填")]
        /*[MaxLength(50)] */ //不設長度自動nvarchar(max)
        [Display(Name = "安心度")]//欄位名稱
        public int? Score { get; set; }


        
        //[Required(ErrorMessage = "{0}必填")]
        //[MaxLength(50)]  //不設長度自動nvarchar(max)
        [Display(Name = "評論")]//欄位名稱
        public string Comment { get; set; }


        
        [Display(Name = "寵物")]
        public int? PetCardId { get; set; }
        
        [ForeignKey("PetCardId")]  //綁關聯   //透過ClassId 查出MyCatalog
        public virtual PetCard PetCards { get; set; }//希望可以直接操縱所屬類別  //虛擬的  //我必須知道我的所屬類別是誰



        [Display(Name = "房型")]
        public int? RoomId { get; set; }
        
        [ForeignKey("RoomId")]  //綁關聯   //透過ClassId 查出MyCatalog
        public virtual Room Rooms { get; set; }//希望可以直接操縱所屬類別  //虛擬的  //我必須知道我的所屬類別是誰



        [Display(Name = "預約單")]
        public int? ReservedListId { get; set; }
        
        [ForeignKey("ReservedListId")]  //綁關聯   //透過ClassId 查出MyCatalog
        public virtual ReservedList ReservedList { get; set; }//希望可以直接操縱所屬類別  //虛擬的  //我必須知道我的所屬類別是誰
        
    }
}