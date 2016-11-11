using System;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

using FJW.Model.MongoDb;

namespace FJW.Wechat.Data
{
    [Table("Activity")]
    public class ActivityModel : BaseModel
    {
        /// <summary>
        /// 键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 奖励类型
        /// </summary>
        public int RewardType { get; set; }

        /// <summary>
        /// 奖励
        /// </summary>
        public long RewardId { get; set; }

        /// <summary>
        /// 产品Id
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public int MaxValue { get; set; }

        /// <summary>
        /// 活动开始时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 活动结束时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 游戏地址
        /// </summary>
        public string GameUrl { get; set; }

        /// <summary>
        /// 活动配置
        /// </summary>
        public string Config { get; set; }
    }
}