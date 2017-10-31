using FJW.Wechat.Cache;
using System;

namespace FJW.Wechat.Activity.ConfigModel
{
    /// <summary>
    /// 中秋大转盘活动配置
    /// </summary>
    public class ZhongQiuDzpConfig
    {
        public static ZhongQiuDzpConfig GetConfig()
        {
            return JsonConfig.GetJson<ZhongQiuDzpConfig>("config/activity.zhongqiudzp.json");
        }
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int ActivityId { get; set; }

        public int RewardId { get; set; }
    }
}
