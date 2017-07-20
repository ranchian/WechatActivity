using System;
using System.ComponentModel.DataAnnotations.Schema;
using FJW.Model.MongoDb;
using MongoDB.Bson.Serialization.Attributes;

namespace FJW.Wechat.Data.Model.Mongo
{
    /// <summary>
    /// 总次数统计（mongo）
    /// </summary>
    [Table("TotalChance")]
    [BsonIgnoreExtraElements]
    public class TotalChanceModel : BaseModel
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
        /// 奖品
        /// </summary>
        public string Prizes { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public long Date { get; set; }

        /// <summary>
        /// 最后统计时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastStatisticsTime { get; set; }


        /// <summary>
        /// 好友编号
        /// </summary>
        public long FriendId { get; set; }

        /// <summary>
        /// 绑定时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? BindDate { get; set; }

        /// <summary>
        /// 微信标识
        /// </summary>
        public string WechatId { get; set; }

        ///<summary>
        /// 微信用户昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string HeadimgUrl { get; set; }

        /// <summary>
        /// 成绩
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 类型 
        /// </summary>
        public int Type{ get; set; }
    }
}
