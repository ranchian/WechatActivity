using System;
using System.ComponentModel.DataAnnotations.Schema;
using FJW.Model.MongoDb;
using MongoDB.Bson.Serialization.Attributes;

namespace FJW.Wechat.Data.Model.Mongo
{
    /// <summary>
    /// 养花活动统计
    /// </summary>
    [Table("Flowers")]
    [BsonIgnoreExtraElements]
    public class FlowersModel : BaseModel
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; }


        /// <summary>
        /// 用户编号
        /// </summary>
        public long MemberId { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 连续天数
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 统计时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastStatisticsTime { get; set; }

    }
}