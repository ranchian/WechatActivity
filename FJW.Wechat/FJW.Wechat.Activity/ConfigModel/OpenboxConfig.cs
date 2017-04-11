using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FJW.Wechat.Cache;

namespace FJW.Wechat.Activity.ConfigModel
{
    public class OpenboxConfig
    {
        public static OpenboxConfig GetConfig()
        {
            return JsonConfig.GetJson<OpenboxConfig>("config/activity.openbox.json");
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
