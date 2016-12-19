using System;


namespace FJW.Wechat.Activity.ConfigModel
{
    public class CollectCardModel
    {

        public DateTime StartTime { get; set; }


        public DateTime EndTime { get; set; }


        public long ActivityId { get; set; }


        public long CashCouponA { get; set; }


        public long CashCouponB { get; set; }


        public long CashCouponC { get; set; }


        public long CashCouponD { get; set; }


        public long RateCouponA { get; set; }


        public long RateCouponB { get; set; }


        public long RateCouponC { get; set; }


        public long RateCouponD { get; set; }

        public long RewardId { get; set; }
    }
}
