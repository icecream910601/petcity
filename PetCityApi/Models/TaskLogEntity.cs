using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PetCityApi1.Models
{
    public class TaskLogEntity
    {
        /// <summary>
        /// 發生時間
        /// </summary>
        public DateTime LogTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 作業名稱
        /// </summary>
        [Required]
        [StringLength(16)]
        public string TaskName { get; set; }
        /// <summary>
        /// 動作
        /// </summary>
        [Required]
        [StringLength(32)]
        public string Action { get; set; }
        /// <summary>
        /// 事件別
        /// </summary>
        public EventTypes EventType { get; set; }
        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }
    }
    /// <summary>
    /// 事件別列舉
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventTypes
    {
        /// <summary>
        /// 開始執行
        /// </summary>
        Start,
        /// <summary>
        /// 執行成功
        /// </summary>
        Succ,
        /// <summary>
        /// 失敗
        /// </summary>
        Fail
    }
}