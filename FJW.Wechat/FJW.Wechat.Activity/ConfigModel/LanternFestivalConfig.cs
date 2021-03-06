﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FJW.Wechat.Activity.ConfigModel
{
    public class LanternFestivalConfig
    {
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        /// <summary>
        /// 活动Id
        /// </summary>
        public long ActivityId { get; set; }
        /// <summary>
        /// 活动key
        /// </summary>
        public string ActivityKey { get; set; }
        /// <summary>
        /// 活动名称
        /// </summary>
        public string ActivityName { get; set; }
        /// <summary>
        /// 限制次数
        /// </summary>
        public int LimitTimes { get; set; }

        public List<Subject> Subjects { get; set; }

        public class Subject
        {
            public int SubjectNo { get; set; }

            public long CouponNo { get; set; }

            public decimal Amount { get; set; }
            public AwardType Type { get; set; }

        }

        public enum AwardType
        {
            CashCoupon = 1,
            RateCoupon = 2,
            ExperienceGold = 3,
            RewardGold = 4
        }
    }
}
