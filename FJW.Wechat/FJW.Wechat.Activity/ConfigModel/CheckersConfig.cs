using System;
using FJW.Wechat.Cache;

namespace FJW.Wechat.Activity.ConfigModel
{
    public class CheckersConfig
    {
        public static CheckersConfig GetConfig()
        {
            return JsonConfig.GetJson<CheckersConfig>("config/activity.checkers.json");
        }

        public DateTime StartTime { get; set; }


        public DateTime EndTime { get; set; }

        /// <summary>
        /// 现金奖励
        /// </summary>
        public long RewardId { get; set; }

        public long ActivityId { get; set; }

        public long Card1 { get; set; }

        public long Card3 { get; set; }

        public long Card4 { get; set; }

        public long Card5 { get; set; }

        public long Card6 { get; set; }

        public long Card7 { get; set; }

        public long Card8 { get; set; }

        public long Card10 { get; set; }

        public long Card11 { get; set; }

        public long Card13 { get; set; }

        public long Card14 { get; set; }

        public long Card15 { get; set; }

        public long Card17 { get; set; }

        public long Card18 { get; set; }

        public long Card19 { get; set; }

        public long Card20 { get; set; }

        public long Card21 { get; set; }

        public long Card23 { get; set; }

        public long Card24 { get; set; }

        public long Card25 { get; set; }

        public long Card26 { get; set; }

        public long Card28 { get; set; }

        public long Card29 { get; set; }

    }
}
