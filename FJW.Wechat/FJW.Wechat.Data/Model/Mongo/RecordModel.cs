using System.ComponentModel.DataAnnotations.Schema;
using FJW.Model.MongoDb;

namespace FJW.Wechat.Data.Model.Mongo
{
    [Table("Record")]
    public class RecordModel : BaseModel
    {
        /// <summary>
        /// 记录Id
        /// </summary>
        public string RecordId { get; set; }

        /// <summary>
        /// 游戏 键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 得分
        /// </summary>
        public int Score { get; set; }

        public int Total { get; set; }

        /// <summary>
        /// 秒数
        /// </summary>
        public decimal Seconds { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 好友邀请数
        /// </summary>
        public int InvitedCount { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        public int Result { get; set; }

        /// <summary>
        /// 用户
        /// </summary>
        public long MemberId { get; set; }

        /// <summary>
        /// 加入类型
        /// </summary>
        public int JoinType { get; set; }

        /// <summary>
        /// 好友邀请Id
        /// </summary>
        public string FriendGameId { get; set; }

        /// <summary>
        /// 微信标识
        /// </summary>
        public string WechatId { get; set; }

        public long ObjectId { get; set; }

        /// <summary>
        /// 游戏数据
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; }

    }
}