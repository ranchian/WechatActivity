using System;
using System.ComponentModel.DataAnnotations.Schema;
using FJW.Model.MongoDb;
using MongoDB.Bson.Serialization.Attributes;

namespace FJW.Wechat.Data.Model.Mongo
{
    [Table("FriendTotalChance")]
    [BsonIgnoreExtraElements]
    public class FriendTotalChanceModel : BaseModel
    {
        public string Key { get; set; }

        public long MemberId { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public long Phone { get; set; }

        /// <summary>
        /// 好友编号
        /// </summary>
        public long FriendId { get; set; }

        /// <summary>
        /// 好友手机号
        /// </summary>
        public long FriendPhone { get; set; }

        /// <summary>
        /// 帮助次数
        /// </summary>
        public int HelpCount { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public long Date { get; set; }
        
        /// <summary>
        /// 最后绑定时间
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? BindDate { get; set; }
       
    }
}