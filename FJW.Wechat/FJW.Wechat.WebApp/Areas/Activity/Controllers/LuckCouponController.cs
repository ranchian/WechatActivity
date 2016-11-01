using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FJW.SDK2Api.CardCoupon;
using FJW.Unit;
using FJW.Wechat.Data;
using FJW.Wechat.WebApp.Models;

namespace FJW.Wechat.WebApp.Areas.Activity.Controllers
{
    public class LuckCouponController : ActivityController
    {
        private const string GameKey = "luckcoupon";
        /// <summary>
        /// 现金奖励Id
        /// </summary>
        private const long RewardId = 0;




        private readonly DateTime _startTime;

        private readonly DateTime _endTime;

        public LuckCouponController()
        {
            _startTime = new DateTime(2016, 11, 4);
#if DEBUG
            _startTime = new DateTime(2016, 8, 4);
#endif

            _endTime = new DateTime(2016, 11, 11);
        }

        /// <summary>
        /// 总次数
        /// </summary>
        /// <returns></returns>
        public ActionResult Total()
        {
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            var userId = UserInfo.Id;


            //
            var activeRepository = new ActivityRepository(DbName, MongoHost);

            var total = activeRepository.Query<TotalChanceModel>(it => it.Key == GameKey && it.MemberId == userId).FirstOrDefault();
            if (total == null)
            {
                var all = GetMemberShares(userId);
                //总次数
                var chances = SumChance(all);

                //create
                total = new TotalChanceModel
                {
                    Key = GameKey,
                    MemberId = userId,
                    Total = chances,
                    Used = 0,
                    NotUsed = 0,
                    LastStatisticsTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now,
                    CreateTime = DateTime.Now,
                    Remark = all.ToJson()
                };
                activeRepository.Add(total);
            }
            else
            {
                //统计间隔 > 30s 
                if ((DateTime.Now - total.LastStatisticsTime).TotalSeconds > 30)
                {
                    var all = GetMemberShares(userId);
                    //总次数
                    var chances = SumChance(all);
                    //if changed then save total
                    if (chances != total.Total)
                    {
                        total.Total = chances;
                        total.Remark = all.ToJson();
                        total.LastStatisticsTime = DateTime.Now;
                        activeRepository.Update(total);
                    }
                }
            }

            var dict = new Dictionary<string, int>();
            dict["chance"] = total.Total;
            dict["used"] = total.Used;
            dict["notUsed"] = total.NotUsed;
            return Json(new ResponseModel { Data = dict });
        }

        /// <summary>
        /// 抽奖
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Play()
        {
          
            var uid = UserInfo.Id;
            if (uid < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            var total = GetChances(uid);
            if (total == null)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.Exception, Message = "请刷新页面后重试" });
            }
            if (total.Total > total.Used)
            {
                //
                int prize;
                decimal money;
                string name;
                var sequnce = Luckdraw(out prize, out money, out name);
                var record = new LuckdrawModel
                {
                    MemberId = uid,
                    Key = GameKey,
                    Phone = UserInfo.Phone,
                    Sequnce = sequnce,
                    Prize = prize,
                    Name = name,
                    Money = money,
                    CreateTime = DateTime.Now
                };

                //
                if (prize < 2000)
                {
                    record.Status = 1;
                    //卡券
                    long activityId;
                    var couponModelId = GetCouponId(prize, out activityId);
                    CardCouponApi.Grant(uid, 2, activityId, couponModelId);
                }
                else if (3000 < prize  )
                {
                    record.Status = 1;
                    //现金
                    new MemberRepository(SqlConnectString).GiveMoney(uid, money, RewardId, sequnce);
                }
                ++total.Used;

                total.NotUsed = total.Total - total.Used;
                total.LastUpdateTime = DateTime.Now;
                var mongoRepository = new ActivityRepository(DbName, MongoHost);
                mongoRepository.Update(total);
                mongoRepository.Add(record);
                return Json(new ResponseModel
                {
                    Data = new
                    {
                        prize,
                        name,
                        money,
                        chance = total.Total,
                        used = total.Used,
                        notUsed = total.NotUsed
                    }
                });
            }
            return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "您没有抽奖机会了" });
        }

        /// <summary>
        /// 首页数据（获奖记录）
        /// </summary>
        /// <returns></returns>
        public ActionResult Records()
        {
            var activityRepository = new ActivityRepository(DbName, MongoHost);
            int rowCount;
            var reocords = activityRepository.Query<LuckdrawModel>(it => it.MemberId > 0 && it.Key == GameKey && it.Status == 1, 20, 0, out rowCount)
                .Select(it=> new  { name= it.Name, phone= StringHelper.CoverPhone(it.Phone), prize = it.Prize}).ToArray();

            bool state;
            int timeMiSeconds;
            var miSeconds = NextGameTimeMilliseconds(out state, out timeMiSeconds);
            return Json(new ResponseModel {Data = new {reocords, miSeconds, timeMiSeconds, state }});
        }

        /// <summary>
        /// 自己的游戏记录
        /// </summary>
        /// <returns></returns>
        public ActionResult SelfRecords()
        {
            var uid = UserInfo.Id;
            if (uid < 1)
            {
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.NotLogged
                });
            }
            var activityRepository = new ActivityRepository(DbName, MongoHost);
            var reocords = activityRepository.Query<LuckdrawModel>(it => it.MemberId == uid && it.Key == GameKey && it.Status == 1)
                .Select(it => new { name = it.Name, phone = StringHelper.CoverPhone(it.Phone), prize = it.Prize }).ToArray();
            return Json(new ResponseModel { Data = new { reocords } });
        }

        #region  private

        /// <summary>
        /// 获取统计次数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private TotalChanceModel GetChances(long userId)
        {
            //
            var activeRepository = new ActivityRepository(DbName, MongoHost);

            var total = activeRepository.Query<TotalChanceModel>(it => it.Key == GameKey && it.MemberId == userId).FirstOrDefault();
            if (total == null)
            {
                var all = GetMemberShares(userId);
                //总次数
                var chances = SumChance(all);

                //create
                total = new TotalChanceModel
                {
                    Key = GameKey,
                    MemberId = userId,
                    Total = chances,
                    Used = 0,
                    NotUsed = 0,
                    LastStatisticsTime = DateTime.Now,
                    LastUpdateTime = DateTime.Now,
                    CreateTime = DateTime.Now,
                    Remark = all.ToJson()
                };
                activeRepository.Add(total);
            }
            else
            {
                //统计间隔 > 30s 
                if ((DateTime.Now - total.LastStatisticsTime).TotalSeconds > 30)
                {
                    var all = GetMemberShares(userId);
                    //总次数
                    var chances = SumChance(all);
                    //if changed then save total
                    if (chances != total.Total)
                    {
                        total.Total = chances;
                        total.Remark = all.ToJson();
                        total.LastStatisticsTime = DateTime.Now;
                        activeRepository.Update(total);
                    }
                }
            }
            return total;
        }


        /// <summary>
        /// 获取用户购买的产品份额
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private IEnumerable<ProductTypeSumShare> GetMemberShares(long userId)
        {
            var sqlRepository = new SqlDataRepository(SqlConnectString);
            return sqlRepository.GetProductTypeShares(userId, _startTime, _endTime);
        }



        /// <summary>
        /// 统计总次数
        /// </summary>
        /// <param name="shares"></param>
        /// <returns></returns>
        private static int SumChance(IEnumerable<ProductTypeSumShare> shares)
        {
            var chance = 0;
            foreach (var share in shares)
            {
                switch (share.ProductTypeId)
                {
                    case 5: //月宝
                        chance = chance + (int)share.Shares / 500;
                        break;

                    case 6: //季宝
                        chance = chance + (int)share.Shares / 300;
                        break;

                    case 7: //双季宝
                        chance = chance + (int)share.Shares / 200;
                        break;

                    case 8: //年宝
                        chance = chance + (int)share.Shares / 100;
                        break;
                }
            }
            return chance;
        }

        /// <summary>
        /// 抽奖
        /// </summary>
        /// <param name="prize">奖品</param>
        /// <param name="money">钱</param>
        /// <param name="name">名称</param>
        /// <returns></returns>
        private static long Luckdraw(out int prize, out decimal money, out string name)
        {
            var sequnce = RedisManager.GetIncrement("Increment:" + GameKey);

            var n = sequnce % 1000;

            if (n < 150)
            {
                money = 0.3m;
                prize = 1001;
                name = "0.3%加息券";
                return sequnce;
            }
            if (n < 150 + 140)
            {
                money = 0.5m;
                prize = 1002;
                name = "0.5%加息券";
                return sequnce;
            }

            if (n < 150 + 140 + 120)
            {
                money = 0.8m;
                prize = 1003;
                name = "0.8%加息券";
                return sequnce;
            }

            if (n < 150 + 140 + 120 + 120)
            {
                money = 1m;
                prize = 1004;
                name = "1%加息券";
                return sequnce;
            }

            if (n < 150 + 140 + 120 + 120 + 110)
            {
                money = 1.5m;
                prize = 1005;
                name = "1.5%加息券";
                return sequnce;
            }

            if (n < 150 + 140 + 120 + 120 + 100)
            {
                money = 1;
                prize = 2006;
                name = "1元现金券";
                return sequnce;
            }

            if (n < 150 + 140 + 120 + 120 + 100 + 80)
            {
                money = 3;
                prize = 2007;
                name = "3元现金券";
                return sequnce;
            }

            if (n < 150 + 140 + 120 + 120 + 100 + 80 + 70)
            {
                money = 5;
                prize = 2008;
                name = "5元现金券";
                return sequnce;
            }

            if (n < 150 + 140 + 120 + 120 + 100 + 80 + 70 + 50)
            {
                money = 8;
                prize = 2009;
                name = "8元现金券";
                return sequnce;
            }

            if (n < 150 + 140 + 120 + 120 + 100 + 80 + 70 + 50 + 30)
            {
                money = 1;
                prize = 3010;
                name = "1元现金红包";
                return sequnce;
            }

            if (n < 150 + 140 + 120 + 120 + 100 + 80 + 70 + 50 + 30 + 14)
            {
                money = 3;
                prize = 3011;
                name = "3元现金红包";
                return sequnce;
            }

            if (n < 150 + 140 + 120 + 120 + 100 + 80 + 70 + 50 + 30 + 14 + 10)
            {
                money = 5;
                prize = 3012;
                name = "5元现金红包";
                return sequnce;
            }

            if (n < 150 + 140 + 120 + 120 + 100 + 80 + 70 + 50 + 30 + 14 + 10 + 3)
            {
                money = 8;
                prize = 3013;
                name = "8元现金红包";
                return sequnce;
            }

            if (n < 150 + 140 + 120 + 120 + 100 + 80 + 70 + 50 + 30 + 14 + 10 + 3 + 2)
            {
                money = 50;
                prize = 3014;
                name = "50元现金红包";
                return sequnce;
            }

            money = 100;
            prize = 3015;
            name = "100元现金红包";
            return sequnce;
        }

        /// <summary>
        /// 获取卡券Id
        /// </summary>
        /// <param name="prize"></param>
        /// <param name="activityId"></param>
        /// <returns></returns>
        private static long GetCouponId(int prize, out long activityId)
        {
            activityId = 10012;
            switch (prize)
            {
                case 1001:
                    return 105;

                case 1002:
                    return 106;

                case 1003:
                    return 107;

                case 1004:
                    return 108;

                case 1005:
                    return 109;

                case 2006:
                    return 100;

                case 2007:
                    return 101;

                case 2008:
                    return 102;

                case 2009:
                    return 103;

            }
            return 0;
        }

        /// <summary>
        /// 距离下次游戏时间的毫秒数
        /// </summary>
        /// <param name="isStart">是否开始了</param>
        /// <param name="timeMiSecond">游戏剩余时间</param>
        /// <returns></returns>
        private static int NextGameTimeMilliseconds(out bool isStart, out int timeMiSecond)
        {
            var dt = DateTime.Now;
            var dt1 = new DateTime(dt.Year, dt.Month, dt.Day, 11, 30, 0);
            var dt2 = new DateTime(dt.Year, dt.Month, dt.Day, 12, 30, 0);
            var dt3 = new DateTime(dt.Year, dt.Month, dt.Day, 20, 00, 0);
            var dt4 = new DateTime(dt.Year, dt.Month, dt.Day, 21, 00, 0);

            //
            if (dt < dt1)
            {
                isStart = false;
                timeMiSecond = 0;
                return (int)(dt1 - dt).TotalMilliseconds;
            }
            if ( dt > dt1 && dt < dt2)
            {
                isStart = true;
                timeMiSecond = (int)(dt2 - dt).TotalMilliseconds;
                return 0;
            }

            if (dt > dt2 && dt < dt3)
            {
                isStart = false;
                timeMiSecond = 0;
                return (int)(dt3 - dt).TotalMilliseconds;
            }
            if (dt> dt3 && dt < dt4)
            {
                isStart = true;
                timeMiSecond = (int) (dt4 - dt).TotalMilliseconds;
                return 0;
            }
            if (dt > dt4)
            {
                isStart = false;
                timeMiSecond = 0;
                return (int)(dt1.AddDays(1) - dt).TotalMilliseconds;
            }
            isStart = true;
            timeMiSecond = 0;
            return 0;
        }

        #endregion

    }
}