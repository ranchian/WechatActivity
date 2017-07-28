using FJW.Wechat.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.Wechat.Activity.ConfigModel
{
    /// <summary>
    /// 砸蛋活动配置
    /// </summary>
    public class EggsConfig
    {
      
            public static EggsConfig GetConfig()
            {
                return JsonConfig.GetJson<EggsConfig>("config/activity.eggs.json");
            }
            public DateTime StartTime { get; set; }

            public DateTime EndTime { get; set; }

            public int ActivityId { get; set; }
    }
}
