

using System;

namespace FJW.Wechat.Data.Model.RDBS
{
    public class SpringDragonRanking
    {
        public long MemberId { get; set; }

        public decimal TotalBuyShares { get; set; }

        public string Phone { get; set; } 

        public DateTime LastBuyTime { get; set; }
    }
}
