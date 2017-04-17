using System;
using System.Linq;
using System.Web.Mvc;
using FJW.SDK2Api.CardCoupon;
using FJW.Unit;
using FJW.Wechat.Activity.ConfigModel;
using FJW.Wechat.Data;
using FJW.Wechat.Data.Model.Mongo;

namespace FJW.Wechat.Activity.Controllers
{
    /// <summary>
    /// 开宝箱
    /// </summary>
    [CrossDomainFilter]
    public class OpenboxController: ActivityController
    {
        private const string GameKey = "openbox";

        private static readonly char[] Squence1 = {
            'D', 'C', 'D', 'E', 'C', 'E', 'D', 'C', 'D', 'C', 'C', 'E', 'D', 'C', 'B', 'E', 'C', 'D', 'D', 'A',
            'D', 'E', 'C', 'E', 'D', 'C', 'B', 'E', 'C', 'E', 'D', 'C', 'D', 'E', 'C', 'E', 'D', 'C', 'B', 'A',
            'C', 'D', 'D', 'C', 'D', 'E', 'C', 'E', 'D', 'C', 'B', 'D', 'C', 'E', 'D', 'C', 'D', 'D', 'C', 'A',
            'D', 'C', 'B', 'E', 'C', 'E', 'D', 'C', 'D', 'D', 'C', 'E', 'D', 'C', 'B', 'E', 'C', 'D', 'D', 'A',
            'D', 'E', 'C', 'D', 'D', 'C', 'B', 'E', 'C', 'D', 'D', 'C', 'D', 'D', 'C', 'E', 'D', 'C', 'B', 'A'
        };

        private static readonly char[] Squence2 = {
            'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D',
            'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D',
            'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D', 'E', 'D',
            'E', 'D', 'E', 'D', 'E', 'D', 'C', 'D', 'C', 'D', 'C', 'D', 'E', 'D', 'C', 'D', 'E', 'D', 'E', 'D',
            'E', 'D', 'C', 'D', 'E', 'D', 'B', 'D', 'E', 'D', 'C', 'D', 'A', 'D', 'B', 'D', 'B', 'D', 'E', 'D'
        };

        public ActionResult Total()
        {
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            var userId = UserInfo.Id;
            var config = OpenboxConfig.GetConfig();
            var activeRepository = GetRepository();
            var total = activeRepository.Query<TotalChanceModel>(it => it.Key == GameKey && it.MemberId == userId).FirstOrDefault();
            var isNew = false;
            if (total == null)
            {
                total = new TotalChanceModel
                {
                    Used = 0,
                    Key = GameKey,
                    MemberId = userId,
                    NotUsed = 0,
                    Total = 0,
                    Prizes = string.Empty
                };

                isNew = true;
            }
            Summary(total, userId, config);
            var times = total.Prizes.Deserialize<OverTimes>()?? new OverTimes();
            if (!isNew)
            {
                activeRepository.Update(total);
            }
            else
            {
                activeRepository.Add(total);
            }
            var shortage = 0;
            if (times.NotUsed < 1)
            {
                shortage = 20 - (total.Used%20);
            }
            return Json(new ResponseModel
            {
                Data = new { notUsedA = total.NotUsed, notUsedB = times.NotUsed , shortage }
            });
        }

        /// <summary>
        /// 开箱
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Open(int type = 1)
        {
            if (UserInfo.Id < 1)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotLogged });
            }
            if (1 > type || type > 2 )
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.NotVerified, Message = "无效的请求"});
            }

            var userId = UserInfo.Id;
            var config = OpenboxConfig.GetConfig();
            

            var activeRepository = GetRepository();

            var total = activeRepository.Query<TotalChanceModel>(it => it.Key == GameKey && it.MemberId == userId).FirstOrDefault();
            var isNew = false;
            if (total == null)
            {
                total = new TotalChanceModel
                {
                    Used = 0,
                    Key = GameKey,
                    MemberId = userId,
                    NotUsed = 0,
                    Total = 0,
                    Prizes = string.Empty
                };
                isNew = true;
            }

            Summary(total, userId, config);

            long couponId = 0;
            long sequnce = 0;

            var name = "";
            var times = total.Prizes.Deserialize<OverTimes>()?? new OverTimes();
            if (type == 1)
            {
                if (total.NotUsed < 1)
                {
                    return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "机会已用完，快快去投资" });
                }
                name = Open1(config, total, out couponId, out sequnce);
                times.Total = total.Used / 20;
                times.NotUsed = times.Total - times.Used;
                if (times.NotUsed < 0)
                {
                    times.NotUsed = 0;
                }
            }
            if (type == 2)
            {
                if (times.NotUsed < 1)
                {
                    return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "机会已用完，快快去投资" });
                }
                name = Open2(config, times, out couponId, out sequnce);
            }

            total.Prizes = times.ToJson();
            var result = CardCouponApi.UserGrant(userId, config.ActivityId, couponId).Data;
            var luckdraw = new LuckdrawModel
            {
                MemberId = userId,
                Key = GameKey,
                Remark = result,
                Name = name,
                Type = type.ToString(),
                Sequnce = sequnce,
                Prize = couponId,
                Phone = UserInfo.Phone,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };
            activeRepository.Add(luckdraw);
            
            total.LastUpdateTime = DateTime.Now;
            var shortage = 0;
            if (times.NotUsed < 1)
            {
                shortage = 20 - (total.Used % 20);
            }
            if (!isNew)
            {
                activeRepository.Update(total);
            }
            else
            {
                activeRepository.Add(total);
            }
            return Json(new ResponseModel { Data = new { name, notUsedA = total.NotUsed, notUsedB = times.NotUsed, shortage } });
        }

        /// <summary>
        /// 计算数量
        /// </summary>
        /// <param name="total"></param>
        /// <param name="userId"></param>
        /// <param name="config"></param>
        private void Summary(TotalChanceModel total, long userId, OpenboxConfig config)
        {
            var sqlRepository = new SqlDataRepository(SqlConnectString);
            if ((DateTime.Now - total.LastStatisticsTime).TotalSeconds > 30)
            {
                var shares = sqlRepository.GetProductTypeShares(userId, config.StartTime, config.EndTime);
                int count = 0;
                foreach (var r in shares)
                {
                    switch (r.ProductTypeId)
                    {
                        case 8:
                            count += (int) r.BuyShares/300;
                            break;
                        case 7:
                            count += (int)r.BuyShares / 600;
                            break;
                        case 6:
                            count += (int)r.BuyShares / 1200;
                            break;
                    }
                }
                
                var totalCnt = count;
                var notUsed = totalCnt - total.Used;
                total.Remark = shares.ToJson();
                if (notUsed < 0)
                {
                    total.NotUsed = 0;
                }
                else
                {
                    total.Total = totalCnt;
                    total.NotUsed = notUsed;
                }

                total.LastStatisticsTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 开银箱
        /// </summary>
        /// <param name="config"></param>
        /// <param name="total"></param>
        /// <param name="couponId"></param>
        /// <param name="sequnce"></param>
        /// <returns></returns>
        private string Open1(OpenboxConfig config, TotalChanceModel total, out long couponId, out long sequnce)
        {
            var name = LuckRaw1(config, out couponId, out sequnce);
            total.NotUsed--;
            total.Used++;
            return name;
        }

        /// <summary>
        /// 开金箱
        /// </summary>
        /// <param name="config"></param>
        /// <param name="times"></param>
        /// <param name="couponId"></param>
        /// <param name="sequnce"></param>
        /// <returns></returns>
        private string Open2(OpenboxConfig config, OverTimes times, out long couponId, out long sequnce)
        {
            var name = LuckRaw2(config, out couponId, out sequnce);
            times.Used++;
            times.NotUsed = times.Total - times.Used;
            if (times.NotUsed < 0)
            {
                times.NotUsed = 0;
            }
            return name;
        }

        /// <summary>
        /// 游戏结果
        /// </summary>
        /// <returns></returns>
        public ActionResult Result()
        {
            var uid = UserInfo.Id;
            if (uid < 1)
            {
                return Json(new ResponseModel
                {
                    ErrorCode = ErrorCode.NotLogged
                });
            }
            var activityRepository = GetRepository();
            var records = activityRepository.Query<LuckdrawModel>(it => it.MemberId == uid && it.Key == GameKey)
                .Select(it => new {
                    name = it.Name,
                    time = it.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToArray();
            return Json(new ResponseModel { Data = records });
        }

        /// <summary>
        /// 排行榜
        /// </summary>
        /// <returns></returns>
        public ActionResult Record()
        {
            int count;
            var activityRepository = GetRepository();
            var records = activityRepository.QueryDesc<LuckdrawModel, DateTime>(it => (it.Key == GameKey || it.Key == "boxword"), it=>it.CreateTime, 20, 1,  out count)
                .Select(it => new {
                    name = it.Name,
                    phone = StringHelper.CoverPhone(it.Phone),
                    time = it.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")
                }).ToArray();
            return Json(new ResponseModel { Data = records });
        }


        private static string LuckRaw1(OpenboxConfig config, out long couponId, out long s)
        {
            var n = RedisManager.GetIncrement("activity:" + GameKey+"1");
            s = n % 100;
            var c = Squence1[s];

            switch (c)
            {
                case 'A':
                    couponId = 0;
                    return "荣事达养生壶";

                case 'B':
                    couponId = 0;
                    return "75ml小甘菊护手霜";

                case 'C':
                    couponId = config.CouponA;
                    return "5%加息券";

                case 'D':
                    couponId = config.CouponB;
                    return "20元现金券";

                case 'E':
                    couponId = config.CouponC;
                    return "5元现金券";
            }
            couponId = -1;
            return "";
        }

        private static string LuckRaw2(OpenboxConfig config, out long couponId, out long s)
        {
            var n = RedisManager.GetIncrement("activity:" + GameKey + "2");
            s = n % 100;
            var c = Squence2[s];

            switch (c)
            {
                case 'A':
                    couponId = 0;
                    return "iPhone7（128G）";

                case 'B':
                    couponId = 0;
                    return "200手机话费";

                case 'C':
                    couponId =  0;
                    return "100元京东购物卡";

                case 'D':
                    couponId = config.CouponD;
                    return "10%加息券";

                case 'E':
                    couponId = config.CouponE;
                    return "30元现金券";
            }
            couponId = -1;
            return "";
        }
    }

    public class OverTimes
    {
        public int Total { get; set; }

        public int Used { get; set; }

        public int NotUsed { get; set; }
    }
}
