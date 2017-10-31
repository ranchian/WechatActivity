using FJW.Wechat.Cache;
using System;

namespace FJW.Wechat.Activity.ConfigModel
{
    /// <summary>
    /// 活动基础配置
    /// </summary>
    public class BaseConfig<T> where T : class
    {
        public static T GetConfig(string jsonUrl)
        {
            return JsonConfig.GetJson<T>(jsonUrl);
        }
        /// <summary>
        /// 活动开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 活动结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 活动编号
        /// </summary>
        public long ActivityId { get; set; }

        /// <summary>
        /// 奖励编号
        /// </summary>
        public int RewardId { get; set; }
    }
}
