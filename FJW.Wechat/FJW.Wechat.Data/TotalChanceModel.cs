using System;
using System.ComponentModel.DataAnnotations.Schema;
using FJW.Model.MongoDb;
using MongoDB.Bson.Serialization.Attributes;

namespace FJW.Wechat.Data
{
    /// <summary>
    /// 总次数统计（mongo）
    /// </summary>
    [Table("TotalChance")]
    public class TotalChanceModel: BaseModel
    {
        public long MemberId { get; set; }

        public string Key { get; set; }

        /// <summary>
        /// 总次数
        /// </summary>
        public int Total { get; set; }
        
        /// <summary>
        /// 已使用次数
        /// </summary>
        public int Used { get; set; }

        /// <summary>
        /// 未使用次数
        /// </summary>
        public int NotUsed { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 最后统计时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastStatisticsTime { get; set; }


    }
}
