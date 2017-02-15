using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.Wechat.Activity.ConfigModel
{
    public class FebruaryConfig
    {
        public DateTime StartTime { get; set; }
        
        public DateTime EndTime { get; set; }
        
        public long ActivityId { get; set; }

        public long RateCouponA { get; set; }

        public long RateCouponB { get; set; }

        public long RateCouponC { get; set; }

        public long RateCouponD { get; set; }
    }
}
