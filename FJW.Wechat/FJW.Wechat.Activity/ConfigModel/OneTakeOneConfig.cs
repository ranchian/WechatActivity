using FJW.Wechat.Cache;
using System;
using System.Collections.Generic;

namespace FJW.Wechat.Activity.ConfigModel
{
    /// <summary>
    /// 一带一路 配置
    /// </summary>
    public class OneTakeOneConfig
    {

        public static OneTakeOneConfig GetConfig()
        {
                return JsonConfig.GetJson<OneTakeOneConfig>("config/activity.onetakeone.json");
        }
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        /// <summary>
        /// 奖励实体
        /// </summary>
        public List<OneTakeOneRecord> OneTakeOneRecordList { get; set; }
    }

    /// <summary>
    /// 奖励发放实体
    /// </summary>
    public class OneTakeOneRecord
    {
        /// <summary>
        /// 成绩
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// 地点
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 奖励
        /// </summary>
        public string Reward { get; set; }
        public Coupon Coupon { get; set; }
    }

    public class Coupon
    {
        public long RateCoupon1 { get; set; }
        public long RateCoupon2 { get; set; }
        public long RateCoupon3 { get; set; }
        public long RateCoupon4 { get; set; }
    }
}
