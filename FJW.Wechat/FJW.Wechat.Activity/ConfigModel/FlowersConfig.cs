using FJW.Wechat.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.Wechat.Activity.ConfigModel
{
    /// <summary>
    /// 养花活动配置
    /// </summary>
    public class FlowersConfig
    {
      
            public static FlowersConfig GetConfig()
            {
                return JsonConfig.GetJson<FlowersConfig>("config/activity.flowers.json");
            }
            public DateTime StartTime { get; set; }

            public DateTime EndTime { get; set; }

            public DateTime StartStaticsTime { get; set; }
            
            public DateTime EndStaticsTime { get; set; }
    }
}
