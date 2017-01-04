using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FJW.SDK2Api;
using FJW.SDK2Api.CardCoupon;
using FJW.Unit;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Cache;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.Mongo;
using FJW.Wechat.Data.Model.RDBS;

namespace FJW.Wechat.Activity.Controllers
{

    [CrossDomainFilter]
    public class XiaonianController : ActivityController
    {
        private const string GameKey = "xiaonian";

        private static readonly List<long> CouponSequnce = new List<long>
        {
            6,5,6,5,6,5,6,5,6,5,2,
            4,7,4,2,4,2,4,6,4,2,5,
            7,4,7,5,6,5,7,4,2,5,12,
            4,15,4,6,5,7,5,6,4,6,4,
            3,4,3,4,6,4,6,4,2,5,12,
            5,1,3,14,4,7,4,8,5,10,3,
            7,4,3,5,7,5,6,3,6,3,8,
            4,3,3,15,3,1,5,11,3,9,5,
            6,4,13,3,2,3,11,5,2,3,3,5
        };
        private static readonly List<long> RewardSequnce = new List<long>
        {
            7,8,5,7,6,7,7,8,7,7,
            5,7,5,6,8,6,5,7,8,6,
            5,7,8,7,5,7,8,6,5,7,
            8,6,5,7,8,6,5,7,8,6,
            5,7,8,6,5,7,8,6,5,7,
            8,6,5,7,4,6,5,7,8,6,
            5,7,8,6,5,7,8,6,7,7,
            7,6,5,7,8,6,5,7,3,4,
            3,8,6,5,7,4,3,8,6,6,
            7,9,2,1,4,7,8,8,5,7
        };

        private static XiaonianConfig GetConfig()
        {
            return JsonConfig.GetJson<XiaonianConfig>("config/activity.xiaonian.json");
        }

        #region 机会


        public ActionResult Chances()
        {
            var userId = UserInfo.Id;
            if (userId < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            var config = GetConfig();

            var total = GetChances(userId, config.StartTime, config.EndTime);
            var dict = new Dictionary<string, int>();
            dict["chance"] = total.Total;
            dict["used"] = total.Used;
            dict["notUsed"] = total.NotUsed;
            return Json(new ResponseModel { Data = dict });

        }

        private TotalChanceModel GetChances(long userId, DateTime startTime, DateTime endTime)
        {
            var activeRepository = new ActivityRepository(DbName, MongoHost);

            var total =
                activeRepository.Query<TotalChanceModel>(it => it.Key == GameKey && it.MemberId == userId)
                    .FirstOrDefault();
            if (total == null)
            {
                var all = Data(userId, startTime, endTime);
                //总次数
                var chances = SumChance(all);

                //create
                total = new TotalChanceModel
                {
                    Key = GameKey,
                    MemberId = userId,
                    Total = chances,
                    Used = 0,
                    NotUsed = chances,
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
                    var all = Data(userId, startTime, endTime);
                    //总次数
                    var chances = SumChance(all);
                    //if changed then save total
                    if (chances != total.Total)
                    {
                        total.Total = chances;
                        total.NotUsed = chances - total.Used;
                        total.Remark = all.ToJson();
                        total.LastStatisticsTime = DateTime.Now;
                        activeRepository.Update(total);
                    }
                }
            }
            return total;
        }

        private int SumChance(ProductTypeSumShare[] rows)
        {
            int count = 0;
            foreach (var r in rows)
            {
                switch (r.ProductTypeId)
                {
                    case 6:
                        count += (int)r.BuyShares / 400;
                        break;
                    case 7:
                        count += (int)r.BuyShares / 200;
                        break;
                    case 8:
                        count += (int)r.BuyShares / 100;
                        break;
                }
            }
            return count;
        }

        public ProductTypeSumShare[] Data(long memberId, DateTime startTime, DateTime endTime)
        {
            var info = new MemberRepository(SqlConnectString).GetBuySummary(memberId, startTime, endTime);
            return info.ToArray();
        }

        #endregion


        #region 刮卡 

        /// <summary>
        /// 刮卡
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Exchange(int type)
        {

            var userId = UserInfo.Id;
            if (userId < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }

            var config = GetConfig();
            var channel = new SqlDataRepository(SqlConnectString).GetMemberChennel(userId);
            if (channel?.Channel != null && channel.Channel.Equals("WQWLCPS", StringComparison.CurrentCultureIgnoreCase) && channel.CreateTime > config.StartTime)
            {
                return Json(new ResponseModel(ErrorCode.Other) { Message = "您无法参与这次活动：WQWLCPS" });
            }
            if (new MemberRepository(SqlConnectString).DisableMemberInvite(userId))
            {
                return Json(new ResponseModel(ErrorCode.Other) { Message = "您无法参与这次活动：特殊的邀请人" });
            }

            if (type == 1)
            {
                string msg;
                var isSuccess = ExchangeCoupon(config, userId, out msg);
                if (isSuccess)
                {
                    return Json(new ResponseModel { Data = msg });
                }
                return Json(new ResponseModel(ErrorCode.Other) { Message = msg });
            }

            if (type == 2)
            {
                string msg;
                var isSuccess = ExchangeReward(config, userId, out msg);
                if (isSuccess)
                {
                    return Json(new ResponseModel { Data = msg });
                }
                return Json(new ResponseModel(ErrorCode.Other) { Message = msg });
            }

            return Json(new ResponseModel(ErrorCode.NotVerified) { Message = "无效的请求" });
        }

        /// <summary>
        /// 卡券
        /// </summary>
        /// <param name="config"></param>
        /// <param name="userId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool ExchangeCoupon(XiaonianConfig config, long userId, out string msg)
        {
            var total = GetChances(userId, config.StartTime, config.EndTime);
            if (total.Total <= total.Used)
            {
                msg = "您没有机会了";
                return false;
            }

            var activeRepository = new ActivityRepository(DbName, MongoHost);
            //卡券
            long activityId;
            string name;
            string type;
            int prizeType;
            decimal money;
            var prizeId = LuckCoupon(config, out activityId, out type, out name, out prizeType, out money);
            var objId = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            string result;
            switch (prizeType)
            {
                case 1:
                    result = new MemberRepository(SqlConnectString).Give(userId, prizeId, config.ProductId, money, objId).ToString();
                    break;
                case 2:
                    result = (CardCouponApi.UserGrant(userId, activityId, prizeId) ?? new ApiResponse()).Data;
                    break;
                default:
                    msg = "无效的兑换";
                    return false;
            }
            var luckdraw = new LuckdrawModel
            {
                MemberId = userId,
                Key = GameKey,
                Remark = result,
                Type = type,
                Name = name,
                Sequnce = objId,
                Prize = (int)prizeId,
                Phone = UserInfo.Phone,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };
            activeRepository.Add(luckdraw);

            total.Used += 1;
            total.NotUsed = total.Total - total.Used;

            total.LastUpdateTime = DateTime.Now;
            activeRepository.Update(total);
            msg = $"{name}{type}";
            return true;
        }

        /// <summary>
        /// 红包
        /// </summary>
        /// <param name="config"></param>
        /// <param name="userId"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool ExchangeReward(XiaonianConfig config, long userId, out string msg)
        {
            var total = GetChances(userId, config.StartTime, config.EndTime);
            if (total.Total <= total.Used + 3)
            {
                msg = "您没有机会了";
                return false;
            }

            var activeRepository = new ActivityRepository(DbName, MongoHost);
            //卡券

            string name;
            string type;

            decimal money;
            var prizeId = LuckReward(config, out type, out name, out money);
            var objId = long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssffff"));
            new MemberRepository(SqlConnectString).GiveMoney(userId, money, prizeId, objId);

            var luckdraw = new LuckdrawModel
            {
                MemberId = userId,
                Key = GameKey,
                Remark = string.Empty,
                Type = type,
                Name = name,
                Money = money,
                Sequnce = objId,
                Prize = (int)prizeId,
                Phone = UserInfo.Phone,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };
            activeRepository.Add(luckdraw);

            total.Used += 3;
            total.NotUsed = total.Total - total.Used;

            total.LastUpdateTime = DateTime.Now;
            activeRepository.Update(total);
            msg = $"{name}{type}";
            return true;
        }

        /// <summary>
        /// 抽卡券
        /// </summary>
        /// <param name="config"></param>
        /// <param name="activityId"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="prizeType"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        private static long LuckCoupon(XiaonianConfig config, out long activityId, out string type, out string name, out int prizeType, out decimal money)
        {
            long prizeId = 0;

            activityId = config.ActivityId;
            money = 0;
            prizeType = 0;
            type = string.Empty;
            var n = RedisManager.GetIncrement($"activity:{GameKey}_Coupon");
            var s = (int)(n % 100);
            var prize = CouponSequnce[s];
            switch (prize)
            {
                case 1:
                    money = 88888;
                    prizeType = 1;
                    type = "体验金";
                    prizeId = config.ExperienceId;
                    break;

                case 2:
                    money = 10000;
                    prizeType = 1;
                    type = "体验金";
                    prizeId = config.ExperienceId;
                    break;

                case 3:
                    money = 8888;
                    prizeType = 1;
                    type = "体验金";
                    prizeId = config.ExperienceId;
                    break;

                case 4:
                    money = 6888;
                    prizeType = 1;
                    type = "体验金";
                    prizeId = config.ExperienceId;
                    break;

                case 5:
                    money = 6666;
                    prizeType = 1;
                    type = "体验金";
                    prizeId = config.ExperienceId;
                    break;

                case 6:
                    money = 5000;
                    prizeType = 1;
                    type = "体验金";
                    prizeId = config.ExperienceId;
                    break;

                case 7:
                    money = 1000;
                    prizeType = 1;
                    type = "体验金";
                    prizeId = config.ExperienceId;
                    break;

                case 8:
                    money = 500;
                    prizeType = 1;
                    type = "体验金";
                    prizeId = config.ExperienceId;
                    break;

                case 9:
                    prizeType = 2;
                    money = 10;
                    type = "现金券";
                    prizeId = config.CashCouponA;
                    break;

                case 10:
                    prizeType = 2;
                    money = 20;
                    type = "现金券";
                    prizeId = config.CashCouponB;
                    break;

                case 11:
                    prizeType = 2;
                    money = 15;
                    type = "现金券";
                    prizeId = config.CashCouponC;
                    break;

                case 12:
                    prizeType = 2;
                    money = 30;
                    type = "现金券";
                    prizeId = config.CashCouponD;
                    break;

                case 13:
                    prizeType = 2;
                    type = "现金券";
                    money = 30;
                    prizeId = config.CashCouponE;
                    break;

                case 14:
                    prizeType = 2;
                    money = 60;
                    type = "现金券";
                    prizeId = config.CashCouponF;
                    break;

                case 15:
                    prizeType = 2;
                    money = 2;
                    name = "%2";
                    type = "加息券";
                    prizeId = config.RateCouponA;
                    return prizeId;
            }
            name = $"{money}元";
            return prizeId;
        }

        /// <summary>
        /// 抽红包
        /// </summary>
        /// <param name="config"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="money"></param>
        /// <returns></returns>
        private static long LuckReward(XiaonianConfig config, out string type, out string name, out decimal money)
        {
            var n = RedisManager.GetIncrement($"activity:{GameKey}_Reward");
            var s = (int)(n % 100);
            var prize = RewardSequnce[s];
            money = 0;
            type = "现金红包";
            switch (prize)
            {
                case 1:
                    money = 500;
                    break;

                case 2:
                    money = 300;
                    break;

                case 3:
                    money = 100;
                    break;

                case 4:
                    money = 60;
                    break;

                case 5:
                    money = 50;
                    break;

                case 6:
                    money = 30;
                    break;

                case 7:
                    money = 20;
                    break;

                case 8:
                    money = 15;
                    break;

                case 9:
                    money = 10;
                    break;

            }
            name = $"{money}元";
            return config.RewardId;
        }

        #endregion


        /// <summary>
        /// 首页数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Record()
        {
            var rows = new ActivityRepository(DbName, MongoHost).Query<LuckdrawModel>(it => it.Key == GameKey).ToList();
            var data = new ResponseModel
            {
                Data = rows.Select(it => new { phone = StringHelper.CoverPhone(it.Phone), type = it.Type, name = it.Name }).ToArray()
            };
            return Json(data);
        }

        /// <summary>
        /// 游戏结果
        /// </summary>
        /// <returns></returns>
        public ActionResult Result()
        {
            var userId = UserInfo.Id;
            if (userId < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            var rows = new ActivityRepository(DbName, MongoHost).Query<LuckdrawModel>(it => it.Key == GameKey && it.MemberId == userId).ToList();
            var data = new ResponseModel
            {
                Data = rows.Select(it => new { time = it.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"), type = it.Type, name = it.Name }).ToArray()
            };
            return Json(data);
        }


    }
}
