using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PetCityApi1.Models
{
    public class ViewModelPet
    {
        //[Required(ErrorMessage = "{0}必填")]
        //[MaxLength(50)] //不設長度自動nvarchar(max)
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
            //[MaxLength(50)] 不設長度自動nvarchar(max)
            [Display(Name = "食物類型")] //欄位名稱
            public List<string> FoodTypes { get; set; }


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
            public List<string> ServiceTypes { get; set; }
        
    }
}