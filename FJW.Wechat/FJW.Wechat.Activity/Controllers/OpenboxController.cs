using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var sqlRepository = new SqlDataRepository(SqlConnectString);
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
                    Total = 0
                };

                isNew = true;
            }

            if ((DateTime.Now - total.LastStatisticsTime).TotalSeconds > 10)
            {
                var cunt = sqlRepository.BuyCount(userId, config.StartTime, config.EndTime);
                var totalCnt = cunt;
                var notUsed = totalCnt - total.Used;
                if (notUsed < 0)
                {
                    total.Remark = $"total:{total.Total} used:{total.Used} notused:{total.NotUsed}";
                    total.Total = totalCnt;
                    total.Used = totalCnt;
                    total.NotUsed = 0;
                }
                else
                {
                    total.Total = totalCnt;
                    total.NotUsed = notUsed;
                }

                total.LastStatisticsTime = DateTime.Now;
            }
            if (!isNew)
            {
                activeRepository.Update(total);
            }
            else
            {
                activeRepository.Add(total);
            }
            return Json(new ResponseModel
            {
                Data = new { total = total.Total, used = total.Used, notUsed = total.NotUsed}
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
            var userId = UserInfo.Id;
            var config = OpenboxConfig.GetConfig();
            var sqlRepository = new SqlDataRepository(SqlConnectString);
            /*
            var channel = sqlRepository.GetMemberChennel(userId);
            if (channel?.Channel != null && channel.Channel.Equals("WQWLCPS", StringComparison.CurrentCultureIgnoreCase) && channel.CreateTime > config.StartTime)
            {
                return Json(new ResponseModel(ErrorCode.Other) { Message = "您属特殊渠道注册用户,无法参与此活动！" });
            }
            if (new MemberRepository(SqlConnectString).DisableMemberInvite(userId, config.StartTime))
            {
                return Json(new ResponseModel(ErrorCode.Other) { Message = "您属特殊渠道注册用户,无法参与此活动！" });
            }
            */
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
                    Total = 0
                };

                isNew = true;
            }

            if ((DateTime.Now - total.LastStatisticsTime).TotalSeconds > 10)
            {
                var cunt = sqlRepository.BuyCount(userId, config.StartTime, config.EndTime);
                var totalCnt = cunt ;
                var notUsed = totalCnt - total.Used;
                if (notUsed < 0)
                {
                    total.Remark = $"total:{total.Total} used:{total.Used} notused:{total.NotUsed}";
                    total.Total = totalCnt;
                    total.Used = totalCnt;
                    total.NotUsed = 0;
                }
                else
                {
                    total.Total = totalCnt;
                    total.NotUsed = notUsed;
                }

                total.LastStatisticsTime = DateTime.Now;
            }
            var take = type == 1 ? 1 : 20;
            if (total.NotUsed < take)
            {
                return Json(new ResponseModel { ErrorCode = ErrorCode.Other, Message = "机会已用完，快快去投资" });
            }
            long couponId;
            long sequnce;
            var name = type == 1 ? LuckRaw1(config, out couponId, out sequnce): LuckRaw2(config, out couponId, out sequnce);
            //var result = GiveCoupon(total.Used, userId, config, out name, out prize, out sequnce);
            var result = CardCouponApi.UserGrant(userId, config.ActivityId, couponId).Data;
            var luckdraw = new LuckdrawModel
            {
                MemberId = userId,
                Key = GameKey,
                Remark = result,
                Name = name,
                Sequnce = sequnce,
                Prize = couponId,
                Phone = UserInfo.Phone,
                CreateTime = DateTime.Now,
                LastUpdateTime = DateTime.Now
            };
            activeRepository.Add(luckdraw);

            total.Used += take;
            total.NotUsed = total.Total - total.Used;

            total.LastUpdateTime = DateTime.Now;

            if (!isNew)
            {
                activeRepository.Update(total);
            }
            else
            {
                activeRepository.Add(total);
            }
            return Json(new ResponseModel { Data = name });
        }

        public ActionResult Records()
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
            var c = Squence1[s];

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
}
