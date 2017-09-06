using FJW.Wechat.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.Wechat.Activity.ConfigModel
{
    /// <summary>
    /// 九月大转盘赚大闸蟹活动配置
    /// </summary>
    public class SeptemberDzpConfig
    {

        public static SeptemberDzpConfig GetConfig()
        {
            return JsonConfig.GetJson<SeptemberDzpConfig>("config/activity.septemberdzp.json");
        }
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int ActivityId { get; set; }

        public int RewardId { get; set; }

        public long RateCouponA { get; set; }

        public long RateCouponB { get; set; }

        public long RateCouponC { get; set; }
    }
}
