using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FJW.Wechat.Cache;

namespace FJW.Wechat.Activity.ConfigModel
{
    /// <summary>
    /// 开宝箱配置
    /// </summary>
    public class BoxWordConfig
    {

        public static BoxWordConfig GetConfig()
        {
            return JsonConfig.GetJson<BoxWordConfig>("config/activity.boxword.json");
        }

        public DateTime StartTime { get; set; }


        public DateTime EndTime { get; set; }


        public long ActivityId { get; set; }


        public long CouponA { get; set; }

        public long CouponB { get; set; }
        
        public long CouponC { get; set; }

        public long CouponD { get; set; }

        public long CouponE { get; set; }

    }
}
