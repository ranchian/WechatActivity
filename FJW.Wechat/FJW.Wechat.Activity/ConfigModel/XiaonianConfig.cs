using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.Wechat.Activity.ConfigModel
{
    /// <summary>
    /// 小年活动配置
    /// </summary>
    public class XiaonianConfig
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        /// <summary>
        /// 活动Id
        /// </summary>
        public long ActivityId { get; set; }

        public long CashCouponA { get; set; }

        public long CashCouponB { get; set; }

        public long CashCouponC { get; set; }

        public long CashCouponD { get; set; }

        public long CashCouponE { get; set; }

        public long CashCouponF { get; set; }

        public long RateCouponA { get; set; }

        /// <summary>
        /// 体验金Id
        /// </summary>
        public long ExperienceId { get; set; }

        /// <summary>
        /// 现金Id
        /// </summary>
        public long RewardId { get; set; }

        /// <summary>
        /// 产品Id
        /// </summary>
        public long ProductId { get; set; }
    }
}
