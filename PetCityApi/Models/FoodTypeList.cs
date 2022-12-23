using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetCityApi1.Models
{
    /// <summary>
    /// 類別舉例
    /// </summary>
    public class FoodTypeList
    {
        public string FoodType { get; set; }
        public List<string> Data { get; set; } = new List<string>();
    }
}