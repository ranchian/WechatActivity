using System.ComponentModel.DataAnnotations.Schema;
using FJW.Model.MongoDb;

namespace FJW.Wechat.Data.Model.Mongo
{
    /// <summary>
    /// 抽奖活动记录
    /// </summary>
    [Table("LuckdrawRecord")]
    public class LuckdrawModel : BaseModel
    {
        public string Key { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long MemberId { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public long Sequnce { get; set; }

        /// <summary>
        /// 奖品
        /// </summary>
        public long Prize { get; set; }

        /// <summary>
        /// 钱
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 奖品名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}